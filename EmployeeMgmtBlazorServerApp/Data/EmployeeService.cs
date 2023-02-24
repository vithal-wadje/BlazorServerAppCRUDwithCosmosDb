using EmployeeMgmtBlazorServerApp.Model;
using Microsoft.Azure.Cosmos;

namespace EmployeeMgmtBlazorServerApp.Data
{
    public class EmployeeService
    {

        // Cosmos DB details, In real use cases, these details should be configured in secure configuraion file.
        private readonly string CosmosDBAccountUri = "https://localhost:8081/";
        private readonly string CosmosDBAccountPrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private readonly string CosmosDbName = "EmployeeManagementDB";
        private readonly string CosmosDbContainerName = "Employees";


        /// <summary>
        /// Commom Container Client, you can also pass the configuration paramter dynamically.
        /// </summary>
        /// <returns> Container Client </returns>
        private Container ContainerClient()
        {

            CosmosClient cosmosDbClient = new CosmosClient(CosmosDBAccountUri, CosmosDBAccountPrimaryKey);
            Container containerClient = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerName);
            return containerClient;

        }

        public async Task AddEmployee(EmployeeModel employee)
        {
            try
            {
                employee.id = Guid.NewGuid();
                var container = ContainerClient();
                var response = await container.CreateItemAsync(employee, new PartitionKey(employee.Department));

               // return Ok(response);
            }
            catch (Exception ex)
            {

                ex.Message.ToString();
            }

        }


        public async Task<List<EmployeeModel>> GetEmployeeDetails()
        {

            List<EmployeeModel> employees = new List<EmployeeModel>();
            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<EmployeeModel> queryResultSetIterator = container.GetItemQueryIterator<EmployeeModel>(queryDefinition);



                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<EmployeeModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (EmployeeModel employee in currentResultSet)
                    {
                        employees.Add(employee);
                    }
                }

               
            }
            catch (Exception ex)
            {

                 ex.Message.ToString();
            }
            return employees;
        }

        public async Task<EmployeeModel> GetEmployeeDetailsById(string? employeeId, string? partitionKey)
        {

            try
            {
                var container = ContainerClient();
                ItemResponse<EmployeeModel> response = await container.ReadItemAsync<EmployeeModel>(employeeId, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (Exception ex)
            {

                throw new Exception("Exception ", ex);
            }

        }

        public async Task UpdateEmployee(EmployeeModel emp)
        {

            try
            {

                var container = ContainerClient();
                ItemResponse<EmployeeModel> res = await container.ReadItemAsync<EmployeeModel>(Convert.ToString(emp.id), new PartitionKey(emp.Department));

                //Get Existing Item
                var existingItem = res.Resource;

                //Replace existing item values with new values 
                existingItem.Name = emp.Name;
                existingItem.Country = emp.Country;
                existingItem.City = emp.City;
                existingItem.Department = emp.Department;
                existingItem.Designation = emp.Designation;
                string? id = Convert.ToString(existingItem.id);
                var updateRes = await container.ReplaceItemAsync(existingItem, id, new PartitionKey(existingItem.Department));

                //return updateRes.Resource;

            }
            catch (Exception ex)
            {

                throw new Exception("Exception", ex);
            }

        }


        public async Task DeleteEmployee(string? empId, string? partitionKey)
        {

            try
            {

                var container = ContainerClient();
                var response = await container.DeleteItemAsync<EmployeeModel>(empId, new PartitionKey(partitionKey));
               
            }
            catch (Exception ex)
            {

                throw new Exception("Exception", ex);
            }
        }

    }
}

