using System.Net;
using System.Text;
using System.Text.Json;

namespace DeviceManagement.Tests;

public class ApiIntegrationTests : IClassFixture<DeviceApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(DeviceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent Json(string json)
        => new(json, Encoding.UTF8, "application/json");

    private static string ValidDeviceJson(Guid id) => $$"""
        {
            "serialNumber": "{{id}}",
            "modelId": "MDL-001",
            "modelName": "ThinkPad X1",
            "manufacturer": "Lenovo",
            "primaryUser": "test@lego.com",
            "operatingSystem": "Windows 11",
            "deviceType": "Laptop",
            "status": "Active"
        }
        """;

    private async Task<Guid> CreateDeviceAsync()
    {
        var id = Guid.NewGuid();
        var response = await _client.PostAsync("/api/devices", Json(ValidDeviceJson(id)));
        response.EnsureSuccessStatusCode();
        return id;
    }

    private static string? ErrorMessage(JsonElement root, string field)
        => root.GetProperty("errors").GetProperty(field)[0].GetString();

    // --- POST: invalid enum values ---

    [Fact]
    public async Task Post_InvalidDeviceType_Returns400WithClearMessage()
    {
        var body = Json($$"""
            {
                "serialNumber": "{{Guid.NewGuid()}}",
                "modelId": "MDL-001",
                "modelName": "ThinkPad X1",
                "manufacturer": "Lenovo",
                "primaryUser": "test@lego.com",
                "operatingSystem": "Windows 11",
                "deviceType": "Tablet",
                "status": "Active"
            }
            """);

        var response = await _client.PostAsync("/api/devices", body);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("'Tablet' is not a valid DeviceType", ErrorMessage(json, "deviceType"));
        Assert.Contains("Laptop, Desktop", ErrorMessage(json, "deviceType"));
    }

    [Fact]
    public async Task Post_InvalidStatus_Returns400WithClearMessage()
    {
        var body = Json($$"""
            {
                "serialNumber": "{{Guid.NewGuid()}}",
                "modelId": "MDL-001",
                "modelName": "ThinkPad X1",
                "manufacturer": "Lenovo",
                "primaryUser": "test@lego.com",
                "operatingSystem": "Windows 11",
                "deviceType": "Laptop",
                "status": "Online"
            }
            """);

        var response = await _client.PostAsync("/api/devices", body);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("'Online' is not a valid Status", ErrorMessage(json, "status"));
        Assert.Contains("Active, Inactive, Retired", ErrorMessage(json, "status"));
    }

    // --- PUT: invalid enum values ---

    [Fact]
    public async Task Put_InvalidDeviceType_Returns400WithClearMessage()
    {
        var id = await CreateDeviceAsync();

        var response = await _client.PutAsync($"/api/devices/{id}",
            Json("""{"deviceType": "Tablet"}"""));
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("'Tablet' is not a valid DeviceType", ErrorMessage(json, "deviceType"));
        Assert.Contains("Laptop, Desktop", ErrorMessage(json, "deviceType"));
    }

    [Fact]
    public async Task Put_InvalidStatus_Returns400WithClearMessage()
    {
        var id = await CreateDeviceAsync();

        var response = await _client.PutAsync($"/api/devices/{id}",
            Json("""{"status": "Broken"}"""));
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("'Broken' is not a valid Status", ErrorMessage(json, "status"));
        Assert.Contains("Active, Inactive, Retired", ErrorMessage(json, "status"));
    }

    // --- GET by primary user ---

    [Fact]
    public async Task GetByPrimaryUser_ReturnsAllDevicesForThatUser()
    {
        var user = $"unique-{Guid.NewGuid()}@lego.com";

        await _client.PostAsync("/api/devices", Json($$"""
            {
                "serialNumber": "{{Guid.NewGuid()}}",
                "modelId": "MDL-001", "modelName": "ThinkPad X1", "manufacturer": "Lenovo",
                "primaryUser": "{{user}}", "operatingSystem": "Windows 11",
                "deviceType": "Laptop", "status": "Active"
            }
            """));
        await _client.PostAsync("/api/devices", Json($$"""
            {
                "serialNumber": "{{Guid.NewGuid()}}",
                "modelId": "MDL-002", "modelName": "OptiPlex", "manufacturer": "Dell",
                "primaryUser": "{{user}}", "operatingSystem": "Windows 10",
                "deviceType": "Desktop", "status": "Inactive"
            }
            """));

        var response = await _client.GetAsync($"/api/devices?primaryUser={Uri.EscapeDataString(user)}");
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, json.GetArrayLength());
        Assert.All(json.EnumerateArray(), d =>
            Assert.Equal(user, d.GetProperty("primaryUser").GetString()));
    }

    [Fact]
    public async Task GetByPrimaryUser_UnknownUser_ReturnsEmptyList()
    {
        var response = await _client.GetAsync($"/api/devices?primaryUser=nobody@lego.com");
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, json.GetArrayLength());
    }

    [Fact]
    public async Task GetByPrimaryUser_MissingQueryParam_Returns400()
    {
        var response = await _client.GetAsync("/api/devices");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Happy path ---

    [Fact]
    public async Task Post_ValidDevice_Returns201()
    {
        var response = await _client.PostAsync("/api/devices", Json(ValidDeviceJson(Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_ExistingDevice_Returns200WithDevice()
    {
        var id = await CreateDeviceAsync();

        var response = await _client.GetAsync($"/api/devices/{id}");
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id.ToString(), json.GetProperty("serialNumber").GetString());
    }

    [Fact]
    public async Task Get_UnknownDevice_Returns404()
    {
        var response = await _client.GetAsync($"/api/devices/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ValidUpdate_Returns200WithUpdatedFields()
    {
        var id = await CreateDeviceAsync();

        var response = await _client.PutAsync($"/api/devices/{id}",
            Json("""{"status": "Retired", "primaryUser": "updated@lego.com"}"""));
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Retired", json.GetProperty("status").GetString());
        Assert.Equal("updated@lego.com", json.GetProperty("primaryUser").GetString());
    }
}
