using AspNetSecurity.Models;
using AspNetSecurity.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSecurity.Controllers
{
    public class ConferenceController : Controller
    {
        private readonly ConferenceRepo _repo;

        public ConferenceController(ConferenceRepo repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Organizer - Conference Overview";
            return View(_repo.GetAll());
        }

        public IActionResult Add()
        {
            ViewBag.Title = "Organizer - Add Conference";
            return View(new ConferenceModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(ConferenceModel model)
        {
            if (ModelState.IsValid)
                _repo.Add(model);

            return RedirectToAction("Index");
        }
    }
}