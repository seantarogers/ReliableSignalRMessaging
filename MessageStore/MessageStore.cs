namespace MessageStore
{
    using System;
    using System.IO;
    using System.Text;

    using Microsoft.Isam.Esent.Interop;

    public  class MessageStore : IMessageStore
    {
        private const string DatabasePath = @"C:\MessageStore\MessageStore.ebd";

        private const string InstancePath = @"C:\MessageStore\";

        private static Instance instance;

        private const string Message = "Message";

        public void AddMessage(Guid messageId)
        {
            CreateDatabase();

            using (var session = new Session(GetInstance()))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                {
                    using (var messageTable = new Table(session, dbid, Message, OpenTableGrbit.None))
                    {
                        using (var updater = new Update(session, messageTable, JET_prep.Insert))
                        {
                            var columnId = Api.GetTableColumnid(session, messageTable, "MessageId");
                            Api.SetColumn(session, messageTable, columnId, messageId);

                            var columnDesc = Api.GetTableColumnid(session, messageTable, "CreateDate");
                            Api.SetColumn(session, messageTable, columnDesc, DateTime.UtcNow);

                            updater.Save();
                        }
                    }

                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }
        }

        public bool MessageExists(Guid messageId)
        {
            CreateDatabase();

            bool exists;
            using (var session = new Session(GetInstance()))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                using (var messageTable = new Table(session, dbid, Message, OpenTableGrbit.None))
                {

                    var messageIdColumn = Api.GetTableColumnid(session, messageTable, "MessageId");
                    var s = Api.RetrieveColumnAsString(session, messageTable, messageIdColumn, Encoding.Unicode);
                    
                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }

            return false;
        }

        private void CreateDatabase()
        {
            if (File.Exists(DatabasePath))
            {
                return;
            }
            
            instance = GetInstance();
            using (var session = new Session(instance))
            {
                
                JET_DBID database;
                Api.JetCreateDatabase(session, DatabasePath, null, out database, CreateDatabaseGrbit.None);

                // create database schema
                using (var transaction = new Transaction(session))
                {
                    JET_TABLEID tableid;
                    Api.JetCreateTable(session, database, Message, 1, 100, out tableid);

                    // ID
                    JET_COLUMNID columnid;
                    Api.JetAddColumn(session, tableid, "Id", new JET_COLUMNDEF
                            {
                                cbMax = 16,
                                coltyp = JET_coltyp.Binary,
                                grbit = ColumndefGrbit.ColumnFixed | ColumndefGrbit.ColumnNotNULL
                            },
                        null,
                        0,
                        out columnid);

                    Api.JetAddColumn(
                        session,
                        tableid,
                        "MessageId",
                        new JET_COLUMNDEF { coltyp = JET_coltyp.Text, cp = JET_CP.Unicode, grbit = ColumndefGrbit.None },
                        null,
                        0,
                        out columnid);

                    Api.JetAddColumn(
                        session,
                        tableid,
                        "DateCreated",
                        new JET_COLUMNDEF { coltyp = JET_coltyp.DateTime, grbit = ColumndefGrbit.None },
                        null,
                        0,
                        out columnid);

                    // Define table indices
                    var indexDef = "+Id\0\0";
                    Api.JetCreateIndex(
                        session,
                        tableid,
                        "id_index",
                        CreateIndexGrbit.IndexPrimary,
                        indexDef,
                        indexDef.Length,
                        100);

                    indexDef = "+MessageId\0\0";
                    Api.JetCreateIndex(
                        session,
                        tableid,
                        "MessageId_index",
                        CreateIndexGrbit.IndexDisallowNull,
                        indexDef,
                        indexDef.Length,
                        100);

                    transaction.Commit(CommitTransactionGrbit.None);
                }

                Api.JetCloseDatabase(session, database, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(session, DatabasePath);
            }
        }

        private static Instance GetInstance()
        {
            lock (InstancePath)
            {
                if (instance != null)
                {
                    return instance;
                }

                var databaseInstance = new Instance(DatabasePath);

                if (File.Exists(DatabasePath))
                {
                    databaseInstance.Init();
                    return databaseInstance;
                }

                CreateInstance(databaseInstance);
                databaseInstance.Init();

                return databaseInstance;
            }
        }

        private static void CreateInstance(Instance databaseInstance)
        {
            databaseInstance.Parameters.CreatePathIfNotExist = true;
            databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
            databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
            databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
            databaseInstance.Parameters.Recovery = true;
            databaseInstance.Parameters.CircularLog = true;
        }
    }
}