using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.LeaveManagement.MVC.Contracts;
using HR.LeaveManagement.MVC.Models;
using HR.LeaveManagement.MVC.Services.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.LeaveManagement.MVC.Controllers

{
    public class LeaveTypesController : Controller
    {
        private readonly ILeaveTypeService _leavetypeRepository;

        public LeaveTypesController(ILeaveTypeService leaveTypeRepository)
        {
            _leavetypeRepository = leaveTypeRepository;
        }
        
        // GET: LeaveTypes
        public async Task<ActionResult> Index()
        {
            var model = await _leavetypeRepository.GetLeaveTypes();
            return View(model);
        }

        // GET: LeaveTypes/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var model = await _leavetypeRepository.GetLeaveTypeDetails(id);
            return View(model);
        }

        // GET: LeaveTypes/Create
        public async Task<ActionResult> Create()
        {
            return View();
        }

        // POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveTypeVM leaveType)
        {
            try
            {
                // TODO: Add insert logic here
                var response = await _leavetypeRepository.CreateLeaveType(leaveType);
                if (response.Success) return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", response.ValidationErrors);
            }
            catch(Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View();
            }
            return View(leaveType);
        }

        // GET: LeaveTypes/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _leavetypeRepository.GetLeaveTypeDetails(id);
            return View();
        }

        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, LeaveTypeVM leaveType)
        {
            try
            {
                // TODO: Add update logic here
                var response = await _leavetypeRepository.UpdateLeaveType(id, leaveType);
                if (response.Success) return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", response.ValidationErrors);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
            }
            return View();
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                var response = await _leavetypeRepository.DeleteLeaveType(id);
                if (response.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", response.ValidationErrors);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
            }
            return BadRequest();
        }
    }
}