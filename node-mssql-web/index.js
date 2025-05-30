const express = require('express');
const sql = require('mssql');

// SQL configuration - replace with your actual connection details
const config = {
  user: 'dbuser',
  password: 'dbpassword',
  server: 'localhost', // or IP address
  database: 'master',
  options: {
    encrypt: false,
    trustServerCertificate: true
  }
};

const app = express();
const port = process.env.PORT || 3000;

// Helper function to run the stored procedure
async function runStoredProcedure() {
  try {
    // create connection pool
    let pool = await sql.connect(config);
    // execute stored procedure; change 'MyProcedure' to your procedure name
    const result = await pool.request().execute('MyProcedure');
    await pool.close();
    return result;
  } catch (err) {
    console.error('Database error:', err);
    throw err;
  }
}

app.get('/', async (req, res) => {
  try {
    const result = await runStoredProcedure();
    res.send(`Stored procedure executed successfully: ${JSON.stringify(result.recordset)}`);
  } catch (err) {
    res.status(500).send('Error executing stored procedure');
  }
});

app.listen(port, () => {
  console.log(`Server running on http://localhost:${port}`);
});
