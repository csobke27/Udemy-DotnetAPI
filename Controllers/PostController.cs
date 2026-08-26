using DotnetAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Models;
using Dapper;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";

            var parameters = new DynamicParameters();
            if(postId != 0)
            {
                sql += " @PostId = @PostIdParam,";
                parameters.Add("PostIdParam", postId);
            }
            if(userId != 0)
            {
                sql += " @UserId = @UserIdParam,";
                parameters.Add("UserIdParam", userId);
            }
            if(!string.IsNullOrEmpty(searchParam) && searchParam.ToLower() != "none")
            {
                sql += " @SearchValue = @SearchValueParam,";
                parameters.Add("SearchValueParam", $"%{searchParam}%");
            }
            sql = _dapper.TrimEndComma(sql);

            return _dapper.LoadDataWithParams<Post>(sql, parameters);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam";
            return _dapper.LoadDataWithParams<Post>(sql, new { UserIdParam = this.User.FindFirst("userId")?.Value });
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost([FromBody] Post post)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert 
                           @UserId = @UserIdParam,
                           @PostTitle = @PostTitleParam,
                           @PostContent = @PostContentParam,";
            var parameters = new DynamicParameters();
            parameters.Add("UserIdParam", this.User.FindFirst("userId")?.Value);
            parameters.Add("PostTitleParam", post.PostTitle);
            parameters.Add("PostContentParam", post.PostContent);
            if(post.PostId > 0)
            {
                sql += " @PostId = @PostIdParam,";
                parameters.Add("PostIdParam", post.PostId);
            }
            sql = _dapper.TrimEndComma(sql);
            
            if (_dapper.ExecuteSQL(sql, parameters))
            {
                return Ok();
            }
            return BadRequest("Failed to add post.");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Delete @PostId = @PostIdParam, @UserId = @UserIdParam";
            var parameters = new DynamicParameters();
            parameters.Add("UserIdParam", this.User.FindFirst("userId")?.Value);
            parameters.Add("PostIdParam", postId);
            if (_dapper.ExecuteSQL(sql, parameters))
            {
                return Ok();
            }
            return BadRequest("Failed to delete post.");
        }
    }
}