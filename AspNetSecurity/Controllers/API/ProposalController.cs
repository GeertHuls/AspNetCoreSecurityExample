using System.Collections.Generic;
using AspNetSecurity.Models;
using AspNetSecurity.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSecurity.Controllers.API
{
    [EnableCors("AllowBankCom")]
    [Route("api/[controller]")]
    public class ProposalController
    {
        private readonly ProposalRepo repo;

        public ProposalController(ProposalRepo repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public IEnumerable<ProposalModel> Get(int conferenceId)
        {
            return repo.GetAllApprovedForConference(conferenceId);
        }
    }
}
