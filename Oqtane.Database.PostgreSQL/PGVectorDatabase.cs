using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Npgsql;
using Oqtane.Databases;

namespace Oqtane.Database.PGVector
{
    public class PGVectorDatabase : DatabaseBase
    {
        private static string _friendlyName => "PGVector";
        private static string _name => "PGVector";

        static PGVectorDatabase()
        {
            Initialize(typeof(PGVectorDatabase));
        }

        public PGVectorDatabase() : base(_name, _friendlyName) { }

        public override string Provider => "Npgsql.EntityFrameworkCore.PostgreSQL";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
        }

        public override int ExecuteNonQuery(string connectionString, string query)
        {
            var conn = new NpgsqlConnection(connectionString);
            var cmd = conn.CreateCommand();
            using (conn)
            {
                PrepareCommand(conn, cmd, query);
                var val = -1;
                try
                {
                    val = cmd.ExecuteNonQuery();
                }
                catch
                {
                    // an error occurred executing the query
                }
                return val;
            }
        }

        public override IDataReader ExecuteReader(string connectionString, string query)
        {
            var conn = new NpgsqlConnection(connectionString);
            var cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .ReplaceService<IHistoryRepository, OqtaneHistoryRepository>();
        }

        private void PrepareCommand(NpgsqlConnection conn, NpgsqlCommand cmd, string query)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
        }
    }
}
