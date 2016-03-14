namespace HubSubscriber.Services
{
    using System;
    using System.IO;

    using Microsoft.Isam.Esent.Interop;

    public class MessageStoreService : IMessageStoreService
    {
        private const string DatabasePath = @"C:\MessageStore\MessageStore.ebd";
        private const string InstancePath = @"C:\MessageStore\";

        //todo abstract esent plumbing, but for the POC we want to see how it works...
        
        static MessageStoreService()
        {
            CreateDatabaseIfRequired();
        }

        public void AddMessageId(Guid messageId)
        {
            if (HasMessageAlreadyBeenProcessed(messageId))
            {
                return;
            }

            InsertMessageIntoDatabase(messageId);
        }

        public bool MessageExists(Guid messageId)
        {
            return HasMessageAlreadyBeenProcessed(messageId);
        }

        private static void InsertMessageIntoDatabase(Guid messageId)
        {
            using (var databaseInstance = new Instance(DatabasePath))
            {
                InitializeDatabaseInstance(databaseInstance);
                InsertMessage(databaseInstance, messageId);
            }
        }

        private static void CreateDatabaseIfRequired()
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

        private static void InsertMessage(Instance databaseInstance, Guid messageId)
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