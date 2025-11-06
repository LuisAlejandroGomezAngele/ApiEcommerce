using System;
using System.Linq;
using ApiEcommerce.Models;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Repository;

public class ProductRepository: IProductRepository
{

    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    //Devuelve todos los productos en ICollection del tipo product
    public ICollection<Product> GetProducts()
    {
        return _db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToList();
    }
    //Devuelve todos los productos de una categoria especifica
    public ICollection<Product> GetProductsInCategory(int categoryId)
    {
        return _db.Products.Include(p => p.Category).Where(p => p.CategoryId == categoryId).ToList();
    }

    //Busca un producto por su nombre
    public ICollection<Product> SearchProduct(string name)
    {
        return _db.Products.Include(p => p.Category).Where(
            p => p.Name.ToLower().Trim().Contains(name.ToLower().Trim()) || 
                 p.Description.ToLower().Trim().Contains(name.ToLower().Trim())).ToList();
    }

    //Busca un producto por su id
    public Product? GetProduct(int id)
    {
        return _db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
    }

    public Product? GetProductByName(string name)
    {
        return _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
    }

    public bool BuyProduct(string name, int quantity)
    {
        if(string.IsNullOrWhiteSpace(name) || quantity <= 0)
        {
            return false;
        }
        var product = _db.Products.FirstOrDefault(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
        if (product == null || product.Stock < quantity)
        {
            return false;
        }
        product.Stock -= quantity;
        _db.Products.Update(product);
        return Save();
    }

    //Verifica si un producto existe por su id
    public bool ProductExists(int id)
    {
        return _db.Products.Any(c => c.ProductId == id);
    }
    //Verifica si un producto existe por su nombre
    public bool ProductExists(string name)
    {
        return _db.Products.Any(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
    }
    //Crea un nuevo producto
    public bool CreateProduct(Product product)
    {
        product.CreationDate = DateTime.Now;
        product.UpdateDate = DateTime.Now;
        _db.Products.Add(product);
        return Save();
    }
    //Actualiza un producto existente
    public bool UpdateProduct(Product product)
    {
        product.CreationDate = DateTime.Now;
        product.UpdateDate = DateTime.Now;
        _db.Products.Update(product);
        return Save();
    }
    //Elimina un producto
    public bool DeleteProduct(Product product)
    {
        _db.Products.Remove(product);
        return Save();
    }
    //Guarda los cambios en la base de datos
    public bool Save()
    {
        return _db.SaveChanges() >= 0 ? true : false;
    }


}