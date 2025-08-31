# EnsekTest

Welcome to the Ensek technical test repository. This guide outlines how to run the projects locally.

---

## ⚙️ Prerequisites

> **Note:** These instructions are tailored for Windows environments. If you're using macOS, SQL Server LocalDB is not supported. You’ll need to configure an alternative database engine such as SQLite, PostgreSQL, MySQL, or run SQL Server in Docker.

Before running the .NET 9 and React projects, ensure the following tools are installed:

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) — tested with Node `v20.14.0`, React version `19.1.1` or higher
- [Git](https://git-scm.com/)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
- **SQL Server LocalDB** (required for the .NET project):
  - Included with Visual Studio or install separately via [SQL Server Express](https://aka.ms/sqlexpress)
  - Verify installation by running:
    ```sh
    sqllocaldb info
    ```

---

## 🌐 Running the .NET API: `EnsekTest.EnergyMeterApi`

1. Open a terminal and navigate to the project root:
    ```sh
    cd EnsekTest.EnergyMeterApi
    ```
2. Restore dependencies:
    ```sh
    dotnet restore
    ```
3. Update the `ConnectionString` in `appsettings.json` to point to your local database instance.
4. Start the API:
    ```sh
    dotnet run
    ```
5. Note the HTTPS URL displayed in the terminal. Append the endpoint path:
    ```
    https://localhost:7188/api/meterreading/meter-reading-uploads
    ```
6. To test file upload via `curl`:
    ```sh
    curl.exe -X POST "https://localhost:7188/api/meterreading/meter-reading-uploads" ^
      -H "Content-Type: multipart/form-data" ^
      -F "file=@C:\path\to\Meter_Reading.csv"
    ```

---

## 🌐 Running the React Client: `EnsekTest.ClientMeterReadingsUploader`

1. Open a terminal and navigate to the project root:
    ```sh
    cd EnsekTest.ClientMeterReadingsUploader
    ```
2. Update the `.env` file:
    ```env
    VITE_UPLOAD_ENDPOINT=https://localhost:7188/api/meterreading/meter-reading-uploads
    ```
3. Install dependencies:
    ```sh
    npm install
    ```
4. Fix any known vulnerabilities:
    ```sh
    npm audit fix
    ```
5. Start the development server:
    ```sh
    npm run dev
    ```
6. The terminal will display the local URL, e.g.:
    ```
    http://localhost:63998/
    ```

---

## 🧪 Running API Tests: `EnsekTest.EnergyMeterApi.Tests`

1. Open a terminal and navigate to the API project root:
    ```sh
    cd EnsekTest.EnergyMeterApi
    ```
2. Run the test suite:
    ```sh
    dotnet test
    ```

---

