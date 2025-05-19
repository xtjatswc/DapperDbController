using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/db")]
public class DapperDbController : ControllerBase
{
    private readonly Dictionary<string, string> _users = new Dictionary<string, string>
    {
        { "QtAdmin", "MGn6Cf8XA55owmya" }
    };

    private bool AuthenticateRequest(HttpRequest request)
    {
        if (!request.Headers.ContainsKey("Authorization"))
            return false;

        var authHeader = request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Basic "))
            return false;

        var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
        var credentialBytes = Convert.FromBase64String(encodedCredentials);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');

        if (credentials.Length != 2)
            return false;

        var username = credentials[0];
        var password = credentials[1];

        return _users.TryGetValue(username, out var storedPassword) &&
               storedPassword == password;
    }

    private IActionResult ChallengeAuth()
    {
        Response.Headers["WWW-Authenticate"] = "Basic realm=\"Database Query Tool\", charset=\"UTF-8\"";
        return Unauthorized();
    }

    [HttpGet("ui")]
    [Produces("text/html")]
    public IActionResult GetUi()
    {
        if (!AuthenticateRequest(Request))
            return ChallengeAuth();

        var authHeader = Request.Headers["Authorization"].ToString();

        var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Database Query Tool</title>
            <meta name=""auth"" content=""{authHeader}"" />
            <style>
                body {{ font-family: Arial, sans-serif; margin: 20px; }}
                .container {{ max-width: 1000px; margin: 0 auto; }}
                textarea, select, input {{ width: 100%; padding: 8px; margin: 5px 0 15px; box-sizing: border-box; }}
                button {{ padding: 10px 15px; background: #4CAF50; color: white; border: none; cursor: pointer; border-radius: 4px; }}
                button:hover {{ background: #45a049; }}
                #result {{ margin-top: 20px; border: 1px solid #ddd; padding: 10px; border-radius: 4px; }}
                table {{ border-collapse: collapse; width: 100%; }}
                th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                th {{ background-color: #f2f2f2; }}
                .example {{ font-size: 0.9em; color: #666; margin-top: -10px; margin-bottom: 15px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Database Query Tool</h1>
                
                <div>
                    <label for='dbType'>Database Type:</label>
                    <select id='dbType' onchange='updateExample()'>
                        <option value='SqlServer'>SQL Server</option>
                        <option value='MySql'>MySQL</option>
                        <option value='PostgreSql'>PostgreSQL</option>
                        <option value='Oracle'>Oracle</option>
                    </select>
                    <div id='connectionExample' class='example'>
                        Example: Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
                    </div>
                </div>
                
                <div>
                    <label for='connectionString'>Connection String:</label>
                    <textarea id='connectionString' rows='3'></textarea>
                </div>
                
                <div>
                    <label for='sqlQuery'>SQL Query:</label>
                    <textarea id='sqlQuery' rows='5'>SELECT * FROM YourTable</textarea>
                </div>
                
                <button onclick='window.executeQuery()'>Execute Query</button>
                <button onclick='window.executeNonQuery()'>Execute Non-Query</button>
                
                <div id='result'></div>
            </div>
            
            <script>
                window.base64Encode = function(str) {{
                    return btoa(encodeURIComponent(str).replace(/%([0-9A-F]{{2}})/g, 
                        function(match, p1) {{
                            return String.fromCharCode('0x' + p1);
                        }}));
                }}
                
                window.getAuthHeader = function() {{
                    const authHeader = document.querySelector('meta[name=""auth""]')?.getAttribute('content');
                    if (!authHeader) {{
                        alert('Authentication required');
                        return null;
                    }}
                    return authHeader;
                }}
                
                window.executeQuery = async function() {{
                    await executeSql('query');
                }}
                
                window.executeNonQuery = async function() {{
                    await executeSql('execute');
                }}
                
                window.updateExample = function() {{
                    const dbType = document.getElementById('dbType').value;
                    const exampleElement = document.getElementById('connectionExample');
                    
                    switch(dbType) {{
                        case 'SqlServer':
                            exampleElement.textContent = 'Example: Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;';
                            break;
                        case 'MySql':
                            exampleElement.textContent = 'Example: Server=localhost;Database=myDataBase;Uid=myUsername;Pwd=myPassword;';
                            break;
                        case 'PostgreSql':
                            exampleElement.textContent = 'Example: Host=myHost;Username=myUsername;Password=myPassword;Database=myDataBase';
                            break;
                        case 'Oracle':
                            exampleElement.textContent = 'Example: Data Source=MyOracleDB;User Id=myUsername;Password=myPassword;';
                            break;
                    }}
                }}
                
                async function executeSql(endpoint) {{
                    const authHeader = getAuthHeader();
                    if (!authHeader) return;
                    
                    const dbType = document.getElementById('dbType').value;
                    const connectionString = document.getElementById('connectionString').value;
                    const sqlQuery = document.getElementById('sqlQuery').value;
                    
                    if (!connectionString || !sqlQuery) {{
                        alert('Please fill in all fields');
                        return;
                    }}
                    
                    const payload = {{
                        EncodedConnectionString: base64Encode(connectionString),
                        EncodedSql: base64Encode(sqlQuery),
                        DatabaseType: dbType
                    }};
                    
                    try {{
                        document.getElementById('result').innerHTML = '<div style=""""color: blue"""">Executing...</div>';
                        
                        const response = await fetch(`${{endpoint}}`, {{
                            method: 'POST',
                            headers: {{ 
                                'Content-Type': 'application/json',
                                'Authorization': authHeader
                            }},
                            body: JSON.stringify(payload)
                        }});
                        
                        if (response.ok) {{
                            if (endpoint === 'query') {{
                                document.getElementById('result').innerHTML = await response.text();
                            }} else {{
                                const result = await response.json();
                                document.getElementById('result').innerHTML = `
                                    <h3>Execution Result</h3>
                                    <p>Affected Rows: ${{result.affectedRows}}</p>
                                `;
                            }}
                        }} else if (response.status === 401) {{
                            window.location.reload();
                        }} else {{
                            const error = await response.text();
                            document.getElementById('result').innerHTML = `
                                <h3 style='color:red'>Error</h3>
                                <pre>${{error}}</pre>
                            `;
                        }}
                    }} catch (error) {{
                        document.getElementById('result').innerHTML = `
                            <h3 style='color:red'>Error</h3>
                            <pre>${{error.message}}</pre>
                        `;
                    }}
                }}
                
                document.addEventListener('DOMContentLoaded', updateExample);
            </script>
        </body>
        </html>";

        return Content(html, "text/html");
    }

    [HttpPost("query")]
    public async Task<IActionResult> ExecuteQuery([FromBody] DbRequest request)
    {
        if (!AuthenticateRequest(Request))
            return ChallengeAuth();

        try
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            var connString = DecodeBase64(request.EncodedConnectionString);
            var sql = DecodeBase64(request.EncodedSql);

            using var connection = CreateConnection(connString, request.DatabaseType);
            connection.Open();

            var result = await connection.QueryAsync(sql);
            var html = GenerateHtmlTable(result);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing query: {ex.Message}");
        }
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteNonQuery([FromBody] DbRequest request)
    {
        if (!AuthenticateRequest(Request))
            return ChallengeAuth();

        try
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            var connString = DecodeBase64(request.EncodedConnectionString);
            var sql = DecodeBase64(request.EncodedSql);

            using var connection = CreateConnection(connString, request.DatabaseType);
            connection.Open();

            var affectedRows = await connection.ExecuteAsync(sql);

            return Ok(new { affectedRows });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing command: {ex.Message}");
        }
    }

    private IDbConnection CreateConnection(string connectionString, string dbType)
    {
        return dbType switch
        {
            "SqlServer" => new SqlConnection(connectionString),
            "MySql" => new MySqlConnection(connectionString),
            "PostgreSql" => new NpgsqlConnection(connectionString),
            "Oracle" => new OracleConnection(connectionString),
            _ => throw new ArgumentException("Unsupported database type")
        };
    }

    private string GenerateHtmlTable(IEnumerable<dynamic> data)
    {
        if (data == null || !data.Any())
            return "<p>No results found</p>";

        var sb = new StringBuilder();
        sb.Append("<table class='result-table'><thead><tr>");

        var firstRow = (IDictionary<string, object>)data.First();
        foreach (var key in firstRow.Keys)
        {
            sb.Append($"<th>{key}</th>");
        }
        sb.Append("</tr></thead><tbody>");

        foreach (var row in data)
        {
            sb.Append("<tr>");
            var dictRow = (IDictionary<string, object>)row;
            foreach (var value in dictRow.Values)
            {
                var displayValue = value?.ToString() ?? "NULL";
                sb.Append($"<td>{displayValue}</td>");
            }
            sb.Append("</tr>");
        }

        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    private string DecodeBase64(string encoded)
    {
        try
        {
            var bytes = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            throw new ArgumentException("Invalid Base64 string");
        }
    }
}

public class DbRequest
{
    public string EncodedConnectionString { get; set; }
    public string EncodedSql { get; set; }
    public string DatabaseType { get; set; }
}