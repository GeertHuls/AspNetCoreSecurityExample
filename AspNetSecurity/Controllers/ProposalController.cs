using AspNetSecurity.Models;
using AspNetSecurity.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSecurity.Controllers
{
    public class ProposalController : Controller
    {
        private readonly ConferenceRepo _conferenceRepo;
        private readonly ProposalRepo _proposalRepo;
        private readonly IDataProtector _protector;

        public ProposalController(ConferenceRepo conferenceRepo, ProposalRepo proposalRepo,
            IDataProtectionProvider dataProtectionProvider, PurposeStringConstants constants)
        {
            _protector = dataProtectionProvider.CreateProtector(constants.ConferenceIdQueryString);

            _conferenceRepo = conferenceRepo;
            _proposalRepo = proposalRepo;
        }

        public IActionResult Index(string conferenceId)
        {
            var deCryptedConferenceId = int.Parse(_protector.Unprotect(conferenceId));
            var conference = _conferenceRepo.GetById(deCryptedConferenceId);
            ViewBag.Title = $"Speaker - Proposals For Conference {conference.Name} {conference.Location}";
            ViewBag.ConferenceId = conferenceId;

            return View(_proposalRepo.GetAllForConference(deCryptedConferenceId));
        }

        public IActionResult AddProposal(int conferenceId)
        {
            ViewBag.Title = "Speaker - Add Proposal";
            return View(new ProposalModel {ConferenceId = conferenceId});
        }

        [HttpPost]
        public IActionResult AddProposal(ProposalModel proposal)
        {
            if (ModelState.IsValid)
                _proposalRepo.Add(proposal);
            return RedirectToAction("Index", new {conferenceId = proposal.ConferenceId});
        }

        public IActionResult Approve(int proposalId)
        {
            var proposal = _proposalRepo.Approve(proposalId);
            return RedirectToAction("Index", new {conferenceId = proposal.ConferenceId});
        }
    }
}