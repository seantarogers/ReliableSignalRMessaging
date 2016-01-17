namespace MessageStore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Isam.Esent.Interop;

    public static class Esent
    {
        //taken from https://github.com/SystemDot/SystemDotServiceBus/blob/master/SystemDotMessaging/Projects/SystemDot.Esent/Esent.cs

        public static JET_DBID OpenDatabase(Session session, string databaseName)
        {
            JET_DBID id;

            Api.JetAttachDatabase(session, databaseName, AttachDatabaseGrbit.None);
            Api.JetOpenDatabase(session, databaseName, null, out id, OpenDatabaseGrbit.None);

            return id;
        }

        public static JET_DBID CreateDatabase(Session session, string databaseName)
        {
            JET_DBID dbid;

            Api.JetCreateDatabase(session, databaseName, null, out dbid, CreateDatabaseGrbit.OverwriteExisting);

            return dbid;
        }

        public static JET_TABLEID CreateTable(JET_DBID dbId, JET_SESID session, string tableName)
        {
            JET_TABLEID tableId;

            Api.JetCreateTable(session, dbId, tableName, 16, 100, out tableId);

            return tableId;
        }

        public static void AddColumn(JET_SESID session, JET_TABLEID tableId, string name, JET_coltyp type, JET_CP cp)
        {
            JET_COLUMNID columnid;

            Api.JetAddColumn(session, tableId, name, new JET_COLUMNDEF { coltyp = type, cp = cp }, null, 0, out columnid);
        }

        public static void AddColumn(JET_SESID session, JET_TABLEID tableId, string name, JET_coltyp type, JET_CP cp, ColumndefGrbit grBit)
        {
            JET_COLUMNID columnid;

            Api.JetAddColumn(session, tableId, name, new JET_COLUMNDEF { coltyp = type, cp = cp, grbit = grBit }, null, 0, out columnid);
        }

        public static IDictionary<string, JET_COLUMNID> GetColumns(Session session, Table table)
        {
            return Api.GetColumnDictionary(session, table);
        }

        public static void SetColumn(Session session, Table table, JET_COLUMNID columnId, Guid toSet)
        {
            Api.SetColumn(session, table, columnId, toSet);
        }

        public static void SetColumn(Session session, Table table, JET_COLUMNID columnId, string toSet, Encoding encoding)
        {
            Api.SetColumn(session, table, columnId, toSet, encoding);
        }

        public static void SetColumn(Session session, Table table, JET_COLUMNID columnId, byte[] toSet)
        {
            Api.SetColumn(session, table, columnId, toSet);
        }

        public static byte[] RetrieveBytesFromColumn(Session session, Table table, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumn(session, table, columnid);
        }

        public static int RetrieveInt32FromColumn(Session session, Table table, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsInt32(session, table, columnid).Value;
        }

        public static int RetrieveAutoIncrementColumn(Session session, Table table, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsInt32(session, table, columnid, RetrieveColumnGrbit.RetrieveCopy).Value;
        }

        public static void CreateIndex(JET_SESID session, JET_TABLEID tableId, string indexName, string keyDescription)
        {
            Api.JetCreateIndex(session, tableId, indexName, CreateIndexGrbit.IndexUnique, keyDescription, keyDescription.Length, 100);
        }

        public static void UsePrimaryIndex(Session session, Table table)
        {
            Api.JetSetCurrentIndex(session, table, null);
        }

        public static void UseIndex(Session session, Table table, string indexName)
        {
            Api.JetSetCurrentIndex(session, table, indexName);
        }

        public static bool TrySetIndexRange(Session session, Table table, SetIndexRangeGrbit rangeFlags)
        {
            return Api.TrySetIndexRange(session, table, rangeFlags);
        }

        public static void SetSearchKey(Session session, Table table, Guid toSet)
        {
            Api.MakeKey(session, table, toSet, MakeKeyGrbit.NewKey);
        }

        public static void SetFirstSearchKey(Session session, Table table, string toSet, Encoding encoding)
        {
            Api.MakeKey(session, table, toSet, encoding, MakeKeyGrbit.NewKey);
        }

        public static void SetSearchKey(Session session, Table table, int toSet)
        {
            Api.MakeKey(session, table, toSet, MakeKeyGrbit.None);
        }

        public static bool TrySearchForEqualToKey(Session session, Table table)
        {
            return Api.TrySeek(session, table, SeekGrbit.SeekEQ);
        }

        public static bool TrySearchForGreaterThanKey(Session session, Table table)
        {
            return Api.TrySeek(session, table, SeekGrbit.SeekGT);
        }

        public static bool TryMoveNext(Session session, Table table)
        {
            return Api.TryMoveNext(session, table);
        }

        public static void CloseTable(JET_SESID session, JET_TABLEID tableId)
        {
            Api.JetCloseTable(session, tableId);
        }
    }
}