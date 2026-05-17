using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Validations;

public static class InvalidModelStateResponseFactory
{
    public static IActionResult Build(ActionContext context)
    {
        // Errors keyed with "$." are specific deserialization failures (e.g. invalid enum value).
        // Prefer those over the generic top-level "field is required" error that appears
        // when the whole body fails to bind.
        var specific = context.ModelState
            .Where(e => e.Key.StartsWith("$.") && e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key[2..],
                e => e.Value!.Errors
                    .Select(x =>
                    {
                        // System.Text.Json appends " Path: ... | LineNumber: ..." — trim it
                        var msg = x.ErrorMessage;
                        var cut = msg.IndexOf(" Path: ");
                        return cut > 0 ? msg[..cut] + "." : msg;
                    })
                    .ToArray()
            );

        var errors = specific.Count > 0
            ? specific
            : context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new { status = 400, errors });
    }
}
