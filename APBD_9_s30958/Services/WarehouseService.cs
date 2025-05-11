using System.Data.Common;
using APBD_9_s30958.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_9_s30958.Services;

public class WarehouseService : IWarehouseService
{
    public readonly IConfiguration _configuration;

    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddWarehouseProduct(WarehouseDTO warehouseDTO)
    {
        await using SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand();
        
        cmd.Connection = conn;
        await conn.OpenAsync();
        
        DbTransaction transaction = await conn.BeginTransactionAsync();
        cmd.Transaction = transaction as SqlTransaction;
        
        try
        { 
            var query = @"Select * from Product where IdProduct = @IdProduct";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@IdProduct", warehouseDTO.IdProduct);
            var productExists = await cmd.ExecuteScalarAsync();
            if (productExists == null)
            {
                throw new Exception("Product not found");
            }
            if (warehouseDTO.Amount <= 0)
            {
                throw new Exception("Wrong amount");
            }
            cmd.Parameters.Clear();
            query = @"Select * from Warehouse where IdWarehouse = @IdWarehouse";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@IdWarehouse", warehouseDTO.IdWarehouse);
            var warehouseExists = await cmd.ExecuteScalarAsync();
            if (warehouseExists == null)
            {
                throw new Exception("Warehouse not found");
            }
            
            cmd.Parameters.Clear();
            query = @"Select IdOrder From ""Order"" Where IdProduct = @IdProduktu AND amount = @Amount";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@IdProduktu", warehouseDTO.IdProduct);
            cmd.Parameters.AddWithValue("@Amount", warehouseDTO.Amount);
            var orderExists = await cmd.ExecuteScalarAsync();
            if (orderExists == null)
            {
                throw new Exception("Order not found");
            }
            int IdOrder =(int) orderExists;
            cmd.Parameters.Clear();
            query = @"Select * from Product_Warehouse where IdOrder = @IdOrder";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@IdOrder", IdOrder);
            var isRealized = await cmd.ExecuteScalarAsync();
            if (isRealized != null)
            {
                throw new Exception("Order was realized");
            }

            cmd.Parameters.Clear();
                query = @"UPDATE ""ORDER"" SET FulfilledAt = @FulfilledAt WHERE IdOrder =@IdOrder";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdOrder", IdOrder);
                await cmd.ExecuteNonQueryAsync();
            
            cmd.Parameters.Clear();
            query = @"Select Price From Product Where IdProduct = @IdProduct";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@IdProduct", warehouseDTO.IdProduct);
            decimal price =(decimal) await cmd.ExecuteScalarAsync();
            
            cmd.Parameters.Clear();
            query = @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) OUTPUT INSERTED.IdProductWarehouse VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@IdWarehouse", warehouseDTO.IdWarehouse);
            cmd.Parameters.AddWithValue("@IdProduct", warehouseDTO.IdProduct);
            cmd.Parameters.AddWithValue("@IdOrder", IdOrder);
            cmd.Parameters.AddWithValue("@Amount", warehouseDTO.Amount);
            cmd.Parameters.AddWithValue("@Price", price*warehouseDTO.Amount);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            int IdProductWarehouse =(int) await cmd.ExecuteScalarAsync();  
            await transaction.CommitAsync();
            return IdProductWarehouse;
        }catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}