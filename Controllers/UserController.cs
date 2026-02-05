using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Aula23.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpGet(Name = "GetAllUsers")]
    public async Task<IActionResult> GetAll()
    {
        // Cria a connectionString
        const string connString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=pedidos";
        
        await using var connection = new NpgsqlConnection(connString);
        // abre a conexão com o banco de dados
        await connection.OpenAsync();

        //cria um novo comando dentro da minha conexão
        NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, email FROM tb_user";

        // executar o comando
        await using var reader = command.ExecuteReader();

        // leia enquanto meu leitor tiver dados
        List<UserDTO> users = new List<UserDTO>();
        while(await reader.ReadAsync())
        {
             // pega o id do usuario
            string id = reader.GetGuid(0).ToString();

            // pega o nome do usuario
            string name = reader.GetString(1);

            string email = reader.GetString(2);

            UserDTO user = new UserDTO
            {
                Name = name,
                Id = id,
                Email = email
            };

            //adicionar na minha lsita de usuários
            users.Add(user);
        }

        // Código 200 OK
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var connString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=pedidos";

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, name, email FROM tb_user WHERE id = @id";
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                UserDTO userObj = new UserDTO {
                    Id = reader.GetGuid(0).ToString(),
                    Name = reader.GetString(1),
                };

                return Ok(userObj);
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost()]
    public async Task<IActionResult> PostUserController([FromBody] CreateUserDTO user)
    {
        var connString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=pedidos";

        var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();

        var sql = """
        INSERT INTO tb_user (name, email, password)
        VALUES(@name, @email, @senha)
        """;

        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("name", user.Nome);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("senha", user.Senha);

        await cmd.ExecuteNonQueryAsync();
        return Created("", null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUserController([FromRoute] Guid id, [FromBody] CreateUserDTO user)
    {
        var connString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=pedidos";

        var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();

        var sql = """
        UPDATE tb_user
        SET Name = @name,
            Email = @email
        WHERE Id = @id;
        """;

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("name", user.Nome);
        cmd.Parameters.AddWithValue("email", user.Email);

        var affected = await cmd.ExecuteNonQueryAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserController([FromRoute] Guid id)
    {
        var connString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=pedidos";

        var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();

        string sql = """
        DELETE FROM tb_user
        WHERE Id = @id;
        """;

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();

        return Ok();
    }

}
