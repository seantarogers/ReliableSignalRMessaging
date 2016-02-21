namespace HubSubscriber.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Isam.Esent.Interop;

    using NUnit.Framework;

    [TestFixture]
    public class MessageStoreServiceTests
    {
        private const string DatabasePath = @"C:\MessageStore\MessageStore.ebd";
        private const string InstancePath = @"C:\MessageStore\";

        [Test, Explicit]
        public void CanBuildDatabase_InsertAMessage_ValidateThatMessageExists_RetrieveAMessage_AndDeleteAMessage()
        {
            // Arrange
            CreateDatabase();

            // Act
            var messageId = InsertMessageIntoDatabase();

            // Assert
            var messageHasAlreadyBeenProcessed = HasMessageAlreadyBeenProcessed(messageId);
            Assert.IsTrue(messageHasAlreadyBeenProcessed);

            // Arrange
            var messageIdsToDelete = GetMessagesToDelete();

            // Act
            DeleteMessages(messageIdsToDelete);

            // Assert
            Assert.AreEqual(messageId, messageIdsToDelete[0]);
            messageIdsToDelete = GetMessagesToDelete();
            Assert.IsFalse(messageIdsToDelete.Any());
        }

        private static Guid InsertMessageIntoDatabase()
        {
            var messageId = Guid.NewGuid();

            using (var databaseInstance = new Instance(DatabasePath))
            {
                InitializeDatabaseInstance(databaseInstance);
                Message(databaseInstance, messageId);
            }
            return messageId;
        }

        private static void CreateDatabase()
        {
            using (var databaseInstance = new Instance(DatabasePath))
            {
                InitializeDatabaseInstance(databaseInstance);

                if (File.Exists(DatabasePath))
                {
                    return;
                }

                using (var session = new Session(databaseInstance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, DatabasePath, null, out dbid, CreateDatabaseGrbit.OverwriteExisting);
                    using (var transaction = new Transaction(session))
                    {
                        JET_TABLEID tableid;
                        Api.JetCreateTable(session, dbid, "Message", 0, 100, out tableid);

                        CreateIdColumn(session, tableid);
                        CreateDateCreatedColumn(session, tableid);
                        CreateIndexes(session, tableid);

                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                    }

                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, DatabasePath);
                }
            }
        }

        private static void CreateIndexes(Session session, JET_TABLEID tableid)
        {
            const string IdIndex = "+Id\0\0";
            Api.JetCreateIndex(session, tableid, "id_index", CreateIndexGrbit.IndexPrimary, IdIndex, IdIndex.Length, 100);

            const string DateCreatedIndex = "+DateCreated\0\0";
            Api.JetCreateIndex(
                session,
                tableid,
                "datecreated_index",
                CreateIndexGrbit.IndexDisallowNull,
                DateCreatedIndex,
                DateCreatedIndex.Length,
                100);
        }

        private static void CreateDateCreatedColumn(Session session, JET_TABLEID tableid)
        {
            JET_COLUMNID dateCreatedColumnid;
            var dateCreatedColumnDefinition = new JET_COLUMNDEF
                                                  {
                                                      coltyp = JET_coltyp.Currency,
                                                      grbit = ColumndefGrbit.ColumnNotNULL
                                                  };
            Api.JetAddColumn(session, tableid, "DateCreated", dateCreatedColumnDefinition, null, 0, out dateCreatedColumnid);
        }

        private static void CreateIdColumn(Session session, JET_TABLEID tableid)
        {
            JET_COLUMNID idColumnId;
            var idColumnDefinition = new JET_COLUMNDEF
                                         {
                                             cbMax = 16,
                                             coltyp = JET_coltyp.Binary,
                                             grbit = ColumndefGrbit.ColumnFixed | ColumndefGrbit.ColumnNotNULL
                                         };
            Api.JetAddColumn(session, tableid, "Id", idColumnDefinition, null, 0, out idColumnId);
        }

        private static List<Guid> GetMessagesToDelete()
        {
            var minDateTime = DateTime.Now;
            List<Guid> messageIdsToDelete;
            using (var databaseInstance = new Instance(DatabasePath))
            {
                InitializeDatabaseInstance(databaseInstance);
                messageIdsToDelete = QueryToGetMessagesToDelete(databaseInstance, minDateTime);
            }
            return messageIdsToDelete;
        }

        private static void DeleteMessages(IEnumerable<Guid> messageIdsToDelete)
        {
            using (var databaseInstance = new Instance(DatabasePath))
            {
                InitializeDatabaseInstance(databaseInstance);

                using (var session = new Session(databaseInstance))
                {
                    JET_DBID dbid;
                    Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                    Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
                    using (var transaction = new Transaction(session))
                    using (var table = new Table(session, dbid, "Message", OpenTableGrbit.None))
                    {
                        Api.JetSetCurrentIndex(session, table, null);
                        foreach (var messageIdToDelete in messageIdsToDelete)
                        {
                            Api.MakeKey(session, table, messageIdToDelete, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(session, table, SeekGrbit.SeekEQ))
                            {
                                Api.JetDelete(session, table);
                            }

                            transaction.Commit(CommitTransactionGrbit.None);
                        }
                    }
                }
            }
        }

        private static List<Guid> QueryToGetMessagesToDelete(Instance databaseInstance, DateTime minDateTime)
        {
            var messageIdsToDelete = new List<Guid>();

            using (var session = new Session(databaseInstance))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                using (var table = new Table(session, dbid, "Message", OpenTableGrbit.None))
                {
                    Api.JetSetCurrentIndex(session, table, "DateCreated_index");
                    Api.MakeKey(session, table, minDateTime, MakeKeyGrbit.NewKey);

                    if (Api.TrySeek(session, table, SeekGrbit.SeekLE))
                    {
                        do
                        {
                            var columnId = Api.GetTableColumnid(session, table, "Id");
                            var messageIdToDelete = Api.RetrieveColumnAsGuid(session, table, columnId) ?? Guid.Empty;
                            messageIdsToDelete.Add(messageIdToDelete);
                        }
                        while (Api.TryMoveNext(session, table));
                    }

                    transaction.Commit(CommitTransactionGrbit.None);
                }

                return messageIdsToDelete;
            }
        }

        private static bool HasMessageAlreadyBeenProcessed(
            Guid messageId)
        {
            using (var databaseInstance = new Instance(DatabasePath))
            {
                InitializeDatabaseInstance(databaseInstance);

                using (var session = new Session(databaseInstance))
                {
                    JET_DBID dbid;
                    Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                    Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
                    bool messageExists;
                    using (var transaction = new Transaction(session))
                    using (var table = new Table(session, dbid, "Message", OpenTableGrbit.None))
                    {
                        Api.JetSetCurrentIndex(session, table, null);
                        Api.MakeKey(session, table, messageId, MakeKeyGrbit.NewKey);

                        if (Api.TrySeek(session, table, SeekGrbit.SeekEQ))
                        {
                            messageExists = true;
                        }
                        else
                        {
                            messageExists = false;
                        }

                        transaction.Commit(CommitTransactionGrbit.None);
                    }

                    return messageExists;
                }
            }
        }

        private static void Message(Instance databaseInstance, Guid messageId)
        {
            using (var session = new Session(databaseInstance))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                using (var table = new Table(session, dbid, "Message", OpenTableGrbit.None))
                using (var update = new Update(session, table, JET_prep.Insert))
                {
                    var columnId = Api.GetTableColumnid(session, table, "Id");
                    Api.SetColumn(session, table, columnId, messageId);

                    var columnDateCreated = Api.GetTableColumnid(session, table, "DateCreated");
                    Api.SetColumn(session, table, columnDateCreated, DateTime.Now.Ticks);

                    update.Save();
                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }
        }

        private static void InitializeDatabaseInstance(Instance databaseInstance)
        {
            databaseInstance.Parameters.CreatePathIfNotExist = true;
            databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
            databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
            databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
            databaseInstance.Parameters.Recovery = true;
            databaseInstance.Parameters.CircularLog = true;
            databaseInstance.Init();
        }
    }
}