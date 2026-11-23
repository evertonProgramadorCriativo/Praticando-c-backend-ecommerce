#!/bin/bash
set -euo pipefail

echo "Aplicando init.sql no SQL Server..."
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -b -I -i /scripts/init.sql
echo "Banco PedidoPostLab pronto."