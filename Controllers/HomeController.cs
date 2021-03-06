﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Controllers
{
    [Authorize]
    public class HomeController: Controller
    {
      private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IEmployeeRepository repository, IHostingEnvironment hostingEnvironment)
        {
            _employeeRepository = repository;
            this.hostingEnvironment = hostingEnvironment;
        }
        public ViewResult Index()
        {
           var model=  _employeeRepository.GetAllEmployees();
            return View(model);
        }
              
        public ViewResult Details(int? id)
        {
           // throw new Exception("Error in Details view");
            Employee employee = _employeeRepository.GetEmployee(id.Value);

            if(employee==null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound",id.Value);

            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
                PageTitle = "Employee Details"

            };
          
            return View(homeDetailsViewModel);
        }


        [HttpGet]

        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]
        
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if(ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);
               
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };
                _employeeRepository.Add(newEmployee);
              return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
         
        }

        [HttpGet]
        
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {

                Id=employee.Id,
                Name=employee.Name,
                Email=employee.Email,
                Department=employee.Department,
                ExistingPhotoPath=employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }

        [HttpPost]
        
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {

                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                if(model.Photo != null) {
                    if (model.ExistingPhotoPath != null)
                    {
                      string filePath=  Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
                      System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }

                
                
                _employeeRepository.Update(employee);
                return RedirectToAction("Index");
            }
            return View();

        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;

            //We can use when uploading multiple file
            //if (model.Photos != null && model.Photos.Count > 0)
            //{
            //    foreach (IFormFile photo in model.Photos)
            //    {
            //        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
            //        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            //        photo.CopyTo(new FileStream(filePath, FileMode.Create));
            //    }
            //}

            if (model.Photo != null)
            {
                
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using(var fileStream= new FileStream(filePath, FileMode.Create))
                        {
                            model.Photo.CopyTo(fileStream); 
                        }
              
                
            }

            return uniqueFileName;
        }
    }
}
