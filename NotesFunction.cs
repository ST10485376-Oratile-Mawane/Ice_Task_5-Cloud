using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Ice_Task_5;
using Ice_Task_5.Services;

namespace Ice_Task_5
{
    public class NotesFunction
    {
        private readonly ILogger<NotesFunction> _logger;
        private readonly NoteStorageService _noteStorageService;

        // Inject the storage service via constructor
        public NotesFunction(ILogger<NotesFunction> logger, NoteStorageService noteStorageService)
        {
            _logger = logger;
            _noteStorageService = noteStorageService;
        }

        private string GetUserId(HttpRequestData req)
        {
            if (req.Headers.TryGetValues("X-User-Id", out var values))
            {
                return values.FirstOrDefault() ?? string.Empty;
            }
            return string.Empty;
        }

        // 1. GET api/notes - Retrieve all notes for the authenticated user
        [Function("GetNotes")]
        public async Task<HttpResponseData> GetNotes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "notes")] HttpRequestData req)
        {
            string userId = GetUserId(req);
            if (string.IsNullOrEmpty(userId)) return await UnauthorizedResponse(req);

            _logger.LogInformation($"Retrieving all notes for user: {userId}");

            // Fetch notes from Table Storage filtered by PartitionKey (userId)
            var notes = await _noteStorageService.GetNotesByUserAsync(userId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(notes);
            return response;
        }

        // 2. POST api/notes - Create a new note
        [Function("CreateNote")]
        public async Task<HttpResponseData> CreateNote([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notes")] HttpRequestData req)
        {
            string userId = GetUserId(req);
            if (string.IsNullOrEmpty(userId)) return await UnauthorizedResponse(req);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var incomingNote = JsonSerializer.Deserialize<NoteTable>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (incomingNote == null)
            {
                var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRes.WriteAsJsonAsync(new { message = "Invalid request body." });
                return badRes;
            }

            // Set mandatory table storage keys
            incomingNote.PartitionKey = userId;
            incomingNote.RowKey = Guid.NewGuid().ToString();
            incomingNote.CreatedAt = DateTime.UtcNow;
            incomingNote.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation($"Creating new note {incomingNote.RowKey} for user: {userId}");

            // Save to Azure Table Storage
            await _noteStorageService.UpsertNoteAsync(incomingNote);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(incomingNote);
            return response;
        }

        // 3. DELETE api/notes/{noteId} - Delete a note
        [Function("DeleteNote")]
        public async Task<HttpResponseData> DeleteNote(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "notes/{noteId}")] HttpRequestData req,
            string noteId)
        {
            string userId = GetUserId(req);
            if (string.IsNullOrEmpty(userId)) return await UnauthorizedResponse(req);

            _logger.LogInformation($"Deleting note {noteId} for user: {userId}");

            await _noteStorageService.DeleteNoteAsync(userId, noteId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { noteId, message = "Note deleted successfully" });
            return response;
        }

        private async Task<HttpResponseData> UnauthorizedResponse(HttpRequestData req)
        {
            var res = req.CreateResponse(HttpStatusCode.BadRequest);
            await res.WriteAsJsonAsync(new { message = "User ID header ('X-User-Id') is missing or invalid." });
            return res;
        }
    }
}