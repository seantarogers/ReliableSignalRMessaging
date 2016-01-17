namespace MessageStore.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;

    using NUnit.Framework;

    [TestFixture]
    public class MessageStoreTests
    {

        [Test, Explicit]
        public void CanCreateDatabase()
        {
            JET_INSTANCE instance;
            JET_SESID sesid;
            JET_DBID dbid;
            JET_TABLEID tableid;

            JET_COLUMNDEF columndef = new JET_COLUMNDEF();
            JET_COLUMNID columnid;

            // Initialize ESENT. Setting JET_param.CircularLog to 1 means ESENT will automatically
            // delete unneeded logfiles. JetInit will inspect the logfiles to see if the last
            // shutdown was clean. If it wasn't (e.g. the application crashed) recovery will be
            // run automatically bringing the database to a consistent state.
            Api.JetCreateInstance(out instance, "instance");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.CircularLog, 1, null);
            Api.JetInit(ref instance);
            Api.JetBeginSession(instance, out sesid, null, null);

            // Create the database. To open an existing database use the JetAttachDatabase and 
            // JetOpenDatabase APIs.
            Api.JetCreateDatabase(sesid, "edbtest.db", null, out dbid, CreateDatabaseGrbit.OverwriteExisting);

            // Create the table. Meta-data operations are transacted and can be performed concurrently.
            // For example, one session can add a column to a table while another session is reading
            // or updating records in the same table.
            // This table has no indexes defined, so it will use the default sequential index. Indexes
            // can be defined with the JetCreateIndex API.
            Api.JetBeginTransaction(sesid);
            Api.JetCreateTable(sesid, dbid, "table", 0, 100, out tableid);
            columndef.coltyp = JET_coltyp.LongText;
            columndef.cp = JET_CP.ASCII;
            Api.JetAddColumn(sesid, tableid, "column1", columndef, null, 0, out columnid);
            Api.JetCommitTransaction(sesid, CommitTransactionGrbit.LazyFlush);

            // Insert a record. This table only has one column but a table can have slightly over 64,000
            // columns defined. Unless a column is declared as fixed or variable it won't take any space
            // in the record unless set. An individual record can have several hundred columns set at one
            // time, the exact number depends on the database page size and the contents of the columns.
            Api.JetBeginTransaction(sesid);
            Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
            string message = "Hello world";
            Api.SetColumn(sesid, tableid, columnid, message, Encoding.ASCII);
            Api.JetUpdate(sesid, tableid);
            Api.JetCommitTransaction(sesid, CommitTransactionGrbit.None); // Use JetRollback() to abort the transaction

            // Retrieve a column from the record. Here we move to the first record with JetMove. By using
            // JetMoveNext it is possible to iterate through all records in a table. Use JetMakeKey and
            // JetSeek to move to a particular record.
            Api.JetMove(sesid, tableid, JET_Move.First, MoveGrbit.None);
            string buffer = Api.RetrieveColumnAsString(sesid, tableid, columnid, Encoding.ASCII);
            Console.WriteLine("{0}", buffer);

            // Terminate ESENT. This performs a clean shutdown.
            Api.JetCloseTable(sesid, tableid);
            Api.JetEndSession(sesid, EndSessionGrbit.None);
            Api.JetTerm(instance);
        }

        [Test, Explicit]
        public void CreateDatabase()
        {
            const string DatabasePath = @"C:\MessageStore\MessageStore.ebd";
            const string InstancePath = @"C:\MessageStore\";

            using (var databaseInstance = new Instance(DatabasePath))
            {
                databaseInstance.Parameters.CreatePathIfNotExist = true;
                databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
                databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
                databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
                databaseInstance.Parameters.Recovery = true;
                databaseInstance.Parameters.CircularLog = true;
                databaseInstance.Init();

                using (var session = new Session(databaseInstance))
                {
                    // create database file
                    JET_DBID database;
                    Api.JetCreateDatabase(session, DatabasePath, null, out database, CreateDatabaseGrbit.None);

                    // create database schema
                    using (var transaction = new Transaction(session))
                    {
                        JET_TABLEID tableid;
                        Api.JetCreateTable(session, database, "Events", 1, 100, out tableid);

                        // ID
                        JET_COLUMNID columnid;
                        Api.JetAddColumn(
                            session,
                            tableid,
                            "Id",
                            new JET_COLUMNDEF
                                {
                                    cbMax = 16,
                                    coltyp = JET_coltyp.Binary,
                                    grbit = ColumndefGrbit.ColumnFixed | ColumndefGrbit.ColumnNotNULL
                                },
                            null,
                            0,
                            out columnid);
                        // Description
                        Api.JetAddColumn(
                            session,
                            tableid,
                            "Description",
                            new JET_COLUMNDEF
                                {
                                    coltyp = JET_coltyp.LongText,
                                    cp = JET_CP.Unicode,
                                    grbit = ColumndefGrbit.None
                                },
                            null,
                            0,
                            out columnid);
                        // Price
                        Api.JetAddColumn(
                            session,
                            tableid,
                            "Price",
                            new JET_COLUMNDEF { coltyp = JET_coltyp.IEEEDouble, grbit = ColumndefGrbit.None },
                            null,
                            0,
                            out columnid);
                        // StartTime
                        Api.JetAddColumn(
                            session,
                            tableid,
                            "StartTime",
                            new JET_COLUMNDEF { coltyp = JET_coltyp.Currency, grbit = ColumndefGrbit.None },
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

                        indexDef = "+Price\0\0";
                        Api.JetCreateIndex(
                            session,
                            tableid,
                            "price_index",
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

            using (var databaseInstance = new Instance(DatabasePath))
            {
                databaseInstance.Init();

            }
        }
        

        [Test, Explicit]
        public void BuildDatabase_AndInsert()
        {
            const string DatabasePath = @"C:\MessageStore\MessageStore.ebd";
            const string InstancePath = @"C:\MessageStore\";
            using (var databaseInstance = new Instance(DatabasePath))
            {
                databaseInstance.Parameters.CreatePathIfNotExist = true;
                databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
                databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
                databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
                databaseInstance.Parameters.Recovery = true;
                databaseInstance.Parameters.CircularLog = true;
                databaseInstance.Init();

                if (!File.Exists(DatabasePath))
                {
                    //need a session to create the database
                    using (var session = new Session(databaseInstance))
                    {
                        JET_DBID dbid;
                        Api.JetCreateDatabase(
                            session,
                            DatabasePath,
                            null,
                            out dbid,
                            CreateDatabaseGrbit.OverwriteExisting);
                        using (var transaction = new Transaction(session))
                        {
                            JET_TABLEID tableid;
                            Api.JetCreateTable(session, dbid, "Message", 0, 100, out tableid);
                            
                            JET_COLUMNID idColumnId;
                            var idColumnDefinition = new JET_COLUMNDEF
                                                         {
                                                             cbMax = 16,
                                                             coltyp = JET_coltyp.Binary,
                                                             grbit =
                                                                 ColumndefGrbit.ColumnFixed | ColumndefGrbit.ColumnNotNULL
                                                         };
                            Api.JetAddColumn(session, tableid, "Id", idColumnDefinition, null, 0, out idColumnId);

                            JET_COLUMNID dateCreatedColumnid;
                            var dateCreatedColumnDefinition = new JET_COLUMNDEF
                                                                  {
                                                                      coltyp = JET_coltyp.Currency,
                                                                      grbit = ColumndefGrbit.ColumnNotNULL
                            };
                            Api.JetAddColumn(session, tableid, "DateCreated", dateCreatedColumnDefinition, null, 0, out dateCreatedColumnid);


                            // Define table indices
                            var indexDef = "+Id\0\0";
                            Api.JetCreateIndex(session, tableid, "id_index",
                                               CreateIndexGrbit.IndexPrimary, indexDef, indexDef.Length, 100);

                            indexDef = "+DateCreated\0\0";
                            Api.JetCreateIndex(session, tableid, "datecreated_index",
                                               CreateIndexGrbit.IndexDisallowNull, indexDef, indexDef.Length, 100);

                            transaction.Commit(CommitTransactionGrbit.LazyFlush);
                            
                        }

                        Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                        Api.JetDetachDatabase(session, DatabasePath);
                    }
                }
            }

            var messageId = Guid.NewGuid();

            using (var databaseInstance = new Instance(DatabasePath))
            {
                databaseInstance.Parameters.CreatePathIfNotExist = true;
                databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
                databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
                databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
                databaseInstance.Parameters.Recovery = true;
                databaseInstance.Parameters.CircularLog = true;
                databaseInstance.Init();
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


            bool messageExists;
            using (var databaseInstance = new Instance(DatabasePath))
            {

                databaseInstance.Parameters.CreatePathIfNotExist = true;
                databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
                databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
                databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
                databaseInstance.Parameters.Recovery = true;
                databaseInstance.Parameters.CircularLog = true;
                databaseInstance.Init();

                using (var session = new Session(databaseInstance))
                {
                    JET_DBID dbid;
                    Api.JetAttachDatabase(session, DatabasePath, AttachDatabaseGrbit.None);
                    Api.JetOpenDatabase(session, DatabasePath, string.Empty, out dbid, OpenDatabaseGrbit.None);
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
                }
            }


            var minDateTime = DateTime.Now;
            
            var messageIdsToDelete = new List<Guid>();
            using (var databaseInstance = new Instance(DatabasePath))
            {

                databaseInstance.Parameters.CreatePathIfNotExist = true;
                databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
                databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
                databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
                databaseInstance.Parameters.Recovery = true;
                databaseInstance.Parameters.CircularLog = true;
                databaseInstance.Init();

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
                }
            }

            using (var databaseInstance = new Instance(DatabasePath))
            {

                databaseInstance.Parameters.CreatePathIfNotExist = true;
                databaseInstance.Parameters.TempDirectory = Path.Combine(InstancePath, "temp");
                databaseInstance.Parameters.SystemDirectory = Path.Combine(InstancePath, "system");
                databaseInstance.Parameters.LogFileDirectory = Path.Combine(InstancePath, "logs");
                databaseInstance.Parameters.Recovery = true;
                databaseInstance.Parameters.CircularLog = true;
                databaseInstance.Init();

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
    }
}