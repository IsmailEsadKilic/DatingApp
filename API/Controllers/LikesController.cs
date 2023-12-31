using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public LikesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpPost("{userName}")] //post to api/likes/userName
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUserId = User.GetUserId(); //get user id
            var likedUser = await _uow.UserRepository.GetUserByUserNameAsync(userName); //get user by userName
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId); //get user with likes

            if (likedUser == null) return NotFound(); //if user is not found, return not found

            if (sourceUser.UserName == userName) return BadRequest("You cannot like yourself"); //if user tries to like themselves, return bad request

            var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id); //get user like

            if (userLike != null) return BadRequest("You already like this user"); //if user already likes this user, return bad request

            userLike = new UserLike //create new user like
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike); //add user like to source user

            if (await _uow.Complete()) return Ok(); //if save is successful, return ok

            return BadRequest("Failed to like user"); //if save is unsuccessful, return bad request

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId(); //get user id
            var users = await _uow.LikesRepository.GetUserLikes(likesParams); //get user likes
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages)); //add pagination header
            return Ok(users); //return users
        }
    }
}