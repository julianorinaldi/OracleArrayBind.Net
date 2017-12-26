using System;
using System.Diagnostics;

namespace OracleArrayBindExample
{
    static class Program
    {
        private const string Col_Cenario = "Cenario";
        private const string Col_Empresa = "Empresa";
        private const string Col_Registro = "Registro";
        private const string Col_Mensagem = "Mensagem";

        private static void Main(string[] args)
        {
            try
            {

                string conenctionString = @"Data Source=srv-servidorOracle:1521/orcl.servidor;User Id=user;Password=password;";
                string tableName = "NOME_TABELA";
                OracleInsertArrayBind oracleInsertArrayBind = new OracleInsertArrayBind(tableName, conenctionString);
                oracleInsertArrayBind.AddColumnName(Col_Cenario, typeof(string));
                oracleInsertArrayBind.AddColumnName(Col_Empresa, typeof(int));
                oracleInsertArrayBind.AddColumnName(Col_Registro, typeof(string));
                oracleInsertArrayBind.AddColumnName(Col_Mensagem, typeof(string));

                for (int i = 0; i < 1000000; i++)
                {
                    oracleInsertArrayBind.AddValuesByColumnName(Col_Cenario, "Cenário - " + i.ToString());
                    oracleInsertArrayBind.AddValuesByColumnName(Col_Empresa, 1);
                    oracleInsertArrayBind.AddValuesByColumnName(Col_Registro, "Registro - " + i.ToString());
                    oracleInsertArrayBind.AddValuesByColumnName(Col_Mensagem, "Mensagem - " + i.ToString());
                }

                oracleInsertArrayBind.ExecuteOracleArrayBind(10000);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        /*
          CREATE TABLE NOME_TABELA
          (
              CENARIO VARCHAR2(100) NOT NULL,
              EMPRESA INT NOT NULL,
              REGISTRO VARCHAR2(100),
              MENSAGEM VARCHAR2(100),
              CONSTRAINT CENARIO_IRPJ_PK PRIMARY KEY (CENARIO)
          )
  
  
          Data Source=srv-servidorOracle:1521/orcl.servidor;User Id=user;Password=password;
        */
    }
}
