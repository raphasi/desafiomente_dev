﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopTFTEC.WebApp.Models;
using ShopTFTEC.WebApp.Services.Contracts;
using ShopTFTEC.WebApp.Roles;
using System.Collections.Generic;

namespace ShopTFTEC.WebApp.Controllers;

[Authorize(Roles = Role.Admin)]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductsController(IProductService productService,
                            ICategoryService categoryService,
                            IWebHostEnvironment webHostEnvironment)
    {
        _productService = productService;
        _categoryService = categoryService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductViewModel>>> Index()
    {

        var result = await _productService.GetAllProducts();

        if (result is null)
            return View("Error");

        return View(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> List(string categoria)
    {
        IEnumerable<ProductViewModel> produtos;
        CategoryViewModel categories;
        string categoriaAtual = string.Empty;

        if (string.IsNullOrEmpty(categoria))
        {
            produtos = await _productService.GetAllProducts();
            categoriaAtual = "Todos os Produtos";
        }
        else
        {
            categories = await _categoryService.GetCategoryByName(categoria);

            if (categories.Products == null)
            {
                produtos = await _productService.GetAllProducts();
                categoriaAtual = "Todos os Produtos";
            }
            else
            {
                produtos = categories.Products;
                categoriaAtual = categoriaAtual;
            }

        }

        var produtoListViewModel = new ProdutoListViewModel
        {
            Produtos = produtos,
            CategoriaAtual = categoriaAtual
        };

        return View(produtoListViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(), "CategoryId", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductViewModel productVM)
    {
        if (ModelState.IsValid)
        {
            var result = await _productService.CreateProduct(productVM);

            if (result != null)
                return RedirectToAction(nameof(Index));
        }
        else
        {
            ViewBag.CategoryId = new SelectList(await
                                 _categoryService.GetAllCategories(), "CategoryId", "Name");
        }
        return View(productVM);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateProduct(int id)
    {
        ViewBag.CategoryId = new SelectList(await
                           _categoryService.GetAllCategories(), "CategoryId", "Name");

        var result = await _productService.FindProductById(id);

        if (result is null)
            return View("Error");

        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProduct(ProductViewModel productVM)
    {
        if (ModelState.IsValid)
        {
            var result = await _productService.UpdateProduct(productVM);

            if (result is not null)
                return RedirectToAction(nameof(Index));
        }
        return View(productVM);
    }

    [HttpGet]
    public async Task<ActionResult<ProductViewModel>> DeleteProduct(int id)
    {
        var result = await _productService.FindProductById(id);

        if (result is null)
            return View("Error");

        return View(result);
    }

    [HttpPost(), ActionName("DeleteProduct")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _productService.DeleteProductById(id);

        if (!result)
            return View("Error");

        return RedirectToAction("Index");
    }
    private async Task<string> GetAccessToken()
    {
        return await HttpContext.GetTokenAsync("access_token");
    }
}
