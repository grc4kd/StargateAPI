# GRC
# Things I found during the coding exercise

# Dapper SQL Parameter support
Dapper doesn't use interpolation strings from C# to validate SQL ADO parameters as it does with the DynamicParameters and Anonymous Parameters. To avoid a SQL injection vulnerability, one of these methods should be called. Since the types of the anonymous parameter properties are shared with ADO.NET, many types of SQL injection attacks involving manipulating the resulting query text sent to the database can be avoided here.

-- Changed GetPersonByName.cs for this example.

