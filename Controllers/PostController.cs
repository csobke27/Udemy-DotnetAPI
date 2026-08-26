using DotnetAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Models;
using DotnetAPI.Dtos;

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

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT PostId, UserId, PostTitle, PostContent, PostCreated, PostUpdated FROM TutorialAppSchema.Posts";
            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]
        public Post? GetPostSingle(int postId)
        {
            string sql = @"SELECT PostId, UserId, PostTitle, PostContent, PostCreated, PostUpdated FROM TutorialAppSchema.Posts WHERE PostId = @PostId";
            return _dapper.LoadDataSingle<Post>(sql, new { PostId = postId });
        }

        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = @"SELECT PostId, UserId, PostTitle, PostContent, PostCreated, PostUpdated FROM TutorialAppSchema.Posts WHERE UserId = @UserId";
            return _dapper.LoadData<Post>(sql, new { UserId = userId });
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"SELECT PostId, UserId, PostTitle, PostContent, PostCreated, PostUpdated FROM TutorialAppSchema.Posts WHERE UserId = @UserId";
            return _dapper.LoadData<Post>(sql, new { UserId = this.User.FindFirst("userId")?.Value });
        }

        [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> GetPostsBySearch(string searchParam)
        {
            string sql = @"SELECT PostId, UserId, PostTitle, PostContent, PostCreated, PostUpdated FROM TutorialAppSchema.Posts WHERE PostTitle LIKE @SearchParam OR PostContent LIKE @SearchParam";
            return _dapper.LoadData<Post>(sql, new { SearchParam = $"%{searchParam}%" });
        }

        [HttpPost("Post")]
        public IActionResult AddPost([FromBody] PostToAddDto postToAdd)
        {
            string sql = @"INSERT INTO TutorialAppSchema.Posts (UserId, PostTitle, PostContent, PostCreated, PostUpdated) VALUES (@UserId, @PostTitle, @PostContent, GETDATE(), GETDATE())";
            bool result = _dapper.ExecuteSQL(sql, new { UserId = this.User.FindFirst("userId")?.Value, PostTitle = postToAdd.PostTitle, PostContent = postToAdd.PostContent });
            if (result)
            {
                return Ok();
            }
            return BadRequest("Failed to add post.");
        }

        [HttpPut("Post")]
        public IActionResult EditPost([FromBody] PostToEditDto postToEdit)
        {
            string sql = @"UPDATE TutorialAppSchema.Posts SET PostTitle = @PostTitle, PostContent = @PostContent, PostUpdated = GETDATE() WHERE PostId = @PostId AND UserId = @UserId";
            bool result = _dapper.ExecuteSQL(sql, new { UserId = this.User.FindFirst("userId")?.Value, PostId = postToEdit.PostId, PostTitle = postToEdit.PostTitle, PostContent = postToEdit.PostContent });
            if (result)
            {
                return Ok();
            }
            return BadRequest("Failed to edit post.");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM TutorialAppSchema.Posts WHERE PostId = @PostId AND UserId = @UserId";
            bool result = _dapper.ExecuteSQL(sql, new { UserId = this.User.FindFirst("userId")?.Value, PostId = postId });
            if (result)
            {
                return Ok();
            }
            return BadRequest("Failed to delete post.");
        }
    }
}