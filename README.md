# OracleArrayBind.Net
How to use Oracle Array Bind to lot of data in .NET C#

Para teste, criar a seguinte tabela:

```
          CREATE TABLE NOME_TABELA
          (
              CENARIO VARCHAR2(100) NOT NULL,
              EMPRESA INT NOT NULL,
              REGISTRO VARCHAR2(100),
              MENSAGEM VARCHAR2(100),
              CONSTRAINT CENARIO_IRPJ_PK PRIMARY KEY (CENARIO)
          )
```
Exemplo da conexão com Oracle
```
Data Source=srv-servidorOracle:1521/orcl.servidor;User Id=user;Password=password;
```
