using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OracleArrayBindExample
{
    public delegate void LogEvent(string message);
    public class OracleInsertArrayBind
    {
        private class DbColumn
        {
            public string ColumnName { get; }
            public Type DataType { get; }
            public DbColumn(string columnName, Type dataType)
            {
                ColumnName = columnName;
                DataType = dataType;
            }
        }

        public event LogEvent LogOracleArrayBind;
        private string TableName { get; }
        private List<DbColumn> ColumnsName { get; }
        private Dictionary<string, List<object>> ColumnsValues { get; }
        private string StringConnection { get; }

        public OracleInsertArrayBind(string tableName, string stringConnection)
        {
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentException("Argumento nulo ou vazio.", nameof(tableName));

            if (String.IsNullOrEmpty(stringConnection))
                throw new ArgumentException("Argumento nulo ou vazio.", nameof(stringConnection));

            StringConnection = stringConnection;
            TableName = tableName.ToUpper();
            ColumnsName = new List<DbColumn>();
            ColumnsValues = new Dictionary<string, List<object>>();
        }

        public void AddColumnName(string columnName)
        {
            AddColumnName(columnName, typeof(string));
        }
        public void AddColumnName(string columnName, Type typeColumn)
        {
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentException("columnName está nulo ou vazio", nameof(columnName));

            columnName = columnName.ToUpper();
            if (!ColumnsName.Exists(x => x.ColumnName.Equals(columnName)))
            {
                Log($"Adicionado ao Oracle ArrayBind coluna: {columnName}, tipo de dados: {typeColumn.ToString()}.");
                ColumnsName.Add(new DbColumn(columnName, typeColumn));
            }
        }

        public void AddValuesByColumnName(string columnName, object value)
        {
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentException("columnName está nulo ou vazio", nameof(columnName));

            columnName = columnName.ToUpper();
            if (!ColumnsName.Exists(x => x.ColumnName.Equals(columnName)))
                throw new Exception(
                    $"Não foi encontrado a coluna {columnName} na configuração de colunas da tabela {TableName}");

            if (!ColumnsValues.Keys.Contains(columnName))
                ColumnsValues[columnName] = new List<object>();

            if (value != null && string.IsNullOrEmpty(value.ToString()))
                value = null;

            ColumnsValues[columnName].Add(value);
        }

        private string GenerateCommandText()
        {
            StringBuilder sbQuery = new StringBuilder();
            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();

            int index = 1;
            foreach (var itemCol in ColumnsName)
            {
                if (sbColumns.Length > 0)
                {
                    sbColumns.Append(", ");
                    sbValues.Append(", ");
                }
                sbColumns.AppendFormat("{0}", itemCol.ColumnName);
                sbValues.AppendFormat(":{0}", index);
                index++;
            }
            sbQuery.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", TableName, sbColumns.ToString(), sbValues.ToString());

            Log($"Gerando estrutura INSERT => Oracle ArrayBind: [{sbQuery.ToString()}]");

            return sbQuery.ToString();
        }

        public List<string> GetColumnsName()
        {
            return ColumnsName.Select(x => x.ColumnName).ToList();
        }

        public void ExecuteOracleArrayBind(int amountBlocks)
        {
            Log($"Iniciando execução de Insert com Blocos de {amountBlocks} registros.");

            double countAllBlocks = (double)ColumnsValues.First().Value.Count;
            int interations = Convert.ToInt32(Math.Ceiling(countAllBlocks / (double)amountBlocks));

            Log($"Serão {interations} iterações, com blocos de {amountBlocks} registros.");

            for (int blockIndex = 0; blockIndex < interations; blockIndex++)
            {
                List<OracleParameter> columnsOracleParameter = new List<OracleParameter>();
                foreach (var item in ColumnsValues)
                {
                    var column = ColumnsName.Single(x => x.ColumnName.Equals(item.Key));
                    var paramOracle = new OracleParameter();
                    paramOracle.OracleDbType = GetType(column.DataType);
                    if (blockIndex == interations - 1)
                        paramOracle.Value = item.Value.Skip(amountBlocks * blockIndex).ToArray();
                    else
                        paramOracle.Value = item.Value.Skip(amountBlocks * blockIndex).Take(amountBlocks).ToArray();
                    columnsOracleParameter.Add(paramOracle);
                }

                int arrayBindCount = ((columnsOracleParameter.First().Value as IList<object>) ?? throw new InvalidOperationException()).Count();
                Log(
                    $"Iniciando inserção: Iteração {blockIndex + 1} de {interations} com blocos de {arrayBindCount} registros.");

                using (OracleConnection con = new OracleConnection(StringConnection))
                {
                    con.Open();
                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = GenerateCommandText();
                    cmd.ArrayBindCount = arrayBindCount;
                    cmd.Parameters.AddRange(columnsOracleParameter.ToArray());
                    cmd.ExecuteNonQuery();
                }

                Log(
                    $"Término da inserção: Iteração {blockIndex + 1} de {interations} com blocos de {arrayBindCount} registros.");
            }
        }

        private static OracleDbType GetType(Type dataType)
        {
            if (dataType == typeof(int))
                return OracleDbType.Int32;
            if (dataType == typeof(Int64))
                return OracleDbType.Int64;
            else if (dataType == typeof(decimal))
                return OracleDbType.Decimal;
            else if (dataType == typeof(DateTime))
                return OracleDbType.Date;

            return OracleDbType.Varchar2;
        }

        private void Log(string message)
        {
            LogOracleArrayBind?.Invoke(message);
        }
    }
}
