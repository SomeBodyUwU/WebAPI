using MySqlConnector;

namespace API;

public class DB
{
    private MySqlConnection _connection = new MySqlConnection("server=mysql-20230529230403.mysql.database.azure.com;port=3306;username=Dmytro@mysql-20230529230403;database=topanime_schema;password=RHJrj500;Allow User Variables=true");

    public MySqlConnection GetConnection()
    {
        return _connection;
    }
}