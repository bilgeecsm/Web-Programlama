using AIRecipeApp.Api.Entities;
using AIRecipeApp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIRecipeApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IOpenAiService _aiService;

        public RecipeController(IRecipeService recipeService, IOpenAiService aiService)
        {
            _recipeService = recipeService;
            _aiService = aiService;
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<List<Recipe>>> Get()
        {
            var recipes = await _recipeService.GetAllAsync();
            return Ok(recipes);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create([FromBody] Recipe recipe)
        {
            if (recipe == null)
                return BadRequest("Tarif bilgisi eksik!");

            await _recipeService.CreateAsync(recipe);
            return CreatedAtAction(nameof(Get), new { id = recipe.Id }, recipe);
        }

        [HttpPost("get-recipe")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetRecipeFromAI([FromBody] List<string> ingredients)
        {
            if (ingredients == null || ingredients.Count == 0)
                return BadRequest("Lütfen en az bir malzeme girin.");

            var recipeText = await _aiService.GetRecipeFromAI(ingredients);
            var recipe = new Recipe
            {
                Title = "AI Önerisi",
                Ingredients = ingredients,
                Instructions = recipeText
            };

            return Ok(recipe);
        }

        [HttpGet("list")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetRecipes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var recipes = await _recipeService.GetByUserIdAsync(userId);
            return Ok(recipes);
        }

        [HttpPost("save-recipe")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SaveRecipe([FromBody] Recipe recipe)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            recipe.UserId = userId;
            await _recipeService.CreateAsync(recipe);
            return Ok(new { message = "Tarif başarıyla kaydedildi!", recipe });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] Recipe recipe)
        {
            if (recipe == null)
                return BadRequest("Tarif bilgisi eksik!");

            var existingRecipe = await _recipeService.GetByIdAsync(id);
            if (existingRecipe == null)
                return NotFound();

            recipe.Id = id;
            await _recipeService.UpdateAsync(recipe);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            if (recipe == null)
                return NotFound();

            await _recipeService.DeleteAsync(id);
            return NoContent();
        }
    }
}