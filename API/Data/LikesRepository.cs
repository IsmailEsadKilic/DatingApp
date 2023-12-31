using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int TargetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, TargetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable(); //get all users
            var likes = _context.Likes.AsQueryable(); //get all likes

            if (likesParams.Predicate == "liked") //if predicate is liked
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId); //get all likes where source user id is equal to user id
                users = likes.Select(like => like.TargetUser); //get all users where target user is equal to user id
            }

            if (likesParams.Predicate == "likedBy") //if predicate is likedBy
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId); //get all likes where target user id is equal to user id
                users = likes.Select(like => like.SourceUser); //get all users where source user is equal to user id
            }

            var LikedUsers = users.Select(user => new LikeDto //return all users as like dto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(), //calculate age
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url, //get photo url
                City = user.City,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(LikedUsers, likesParams.PageNumber, likesParams.PageSize); //return paged list of liked users
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers) //include liked users
                .FirstOrDefaultAsync(x => x.Id == userId); //get first user where id is equal to user id
        }
    }
}