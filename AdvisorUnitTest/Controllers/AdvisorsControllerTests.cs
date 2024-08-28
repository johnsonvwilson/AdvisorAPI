using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AdvisorAPI.Controllers;
using AdvisorAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Xml.Linq;

public class AdvisorsControllerTests
{
    private readonly AdvisorsController _controller;
    private readonly AdvisorDbContext _context;

    public AdvisorsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AdvisorDbContext>()
            .UseInMemoryDatabase(databaseName: "TestAdvisorDB")
            .Options;

        _context = new AdvisorDbContext(options);
        _controller = new AdvisorsController(_context);

        // Ensure the in-memory database is empty
        _context.Advisors.RemoveRange(_context.Advisors);
        _context.SaveChanges();

        // Seed the database with test data
        _context.Advisors.Add(new Advisor { Id = 1, Name = "John Doe", SIN = "123456789", Address = "123 Street", Phone = "12345678", HealthStatus = "Green" });
        _context.Advisors.Add(new Advisor { Id = 2, Name = "Jane Doe", SIN = "987654321", Address = "456 Avenue", Phone = "87654321", HealthStatus = "Yellow" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAdvisor_ShouldReturnAdvisor_WhenAdvisorExists()
    {
        // Arrange
        int advisorId = 25;
        var advisor = new Advisor { Id = advisorId, Name = "John Doe", SIN = "123456789", Address = "123 Street", Phone = "12345678", HealthStatus = "Green" };
        _context.Advisors.Add(advisor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAdvisor(advisorId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var maskedAdvisor = okResult.Value;

        Assert.Equal(advisorId, maskedAdvisor.GetType().GetProperty("Id").GetValue(maskedAdvisor, null));
        Assert.Equal("John Doe", maskedAdvisor.GetType().GetProperty("Name").GetValue(maskedAdvisor, null));
        Assert.Equal("*****6789", maskedAdvisor.GetType().GetProperty("SIN").GetValue(maskedAdvisor, null));
        Assert.Equal("123 Street", maskedAdvisor.GetType().GetProperty("Address").GetValue(maskedAdvisor, null));
        Assert.Equal("****5678", maskedAdvisor.GetType().GetProperty("Phone").GetValue(maskedAdvisor, null));
        Assert.Equal("Green", maskedAdvisor.GetType().GetProperty("HealthStatus").GetValue(maskedAdvisor, null));
    }

    [Fact]
    public async Task GetAdvisor_ShouldReturnNotFound_WhenAdvisorDoesNotExist()
    {
        // Act
        var result = await _controller.GetAdvisor(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnCreatedAdvisor()
    {
        // Arrange
        var advisor = new Advisor { Id = 35,Name = "Test Advisor", SIN = "111222333", Address = "789 Road", Phone = "11223344" };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = createdAtActionResult.Value;
        // var returnValue= Assert.IsType<Advisor>(createdAtActionResult.Value);
        // var returnValue = Assert.IsType<Advisor>(createdAtActionResult.Value);
        Assert.Equal(35, returnValue.GetType().GetProperty("Id").GetValue(returnValue));
        Assert.Equal("Test Advisor", returnValue.GetType().GetProperty("Name").GetValue(returnValue));
        Assert.Equal("789 Road", returnValue.GetType().GetProperty("Address").GetValue(returnValue));
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnBadRequest_WhenSINIsNotUnique()
    {
        // Arrange
        var advisor = new Advisor { Name = "Duplicate SIN", SIN = "123456789", Address = "Some Address", Phone = "12345678" };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("SIN must be unique.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateAdvisor_ShouldReturnNoContent_WhenAdvisorIsUpdatedSuccessfully()
    {
        // Arrange
        var advisor = new Advisor { Id = 1, Name = "Updated Name", SIN = "123456789", Address = "Updated Address", Phone = "12345678", HealthStatus = "Green" };

        // Act
        var result = await _controller.UpdateAdvisor(1, advisor);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAdvisor_ShouldReturnBadRequest_WhenIdDoesNotMatch()
    {
        // Arrange
        var advisor = new Advisor { Id = 2, Name = "Wrong ID", SIN = "987654321", Address = "Some Address", Phone = "87654321", HealthStatus = "Yellow" };

        // Act
        var result = await _controller.UpdateAdvisor(1, advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestResult>(result); 
       // Assert.Equal("Advisor ID mismatch.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteAdvisor_ShouldReturnNoContent_WhenAdvisorIsDeletedSuccessfully()
    {
        // Act
        var result = await _controller.DeleteAdvisor(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify that the advisor was deleted
        var advisor = await _context.Advisors.FindAsync(1);
        Assert.Null(advisor);
    }

    [Fact]
    public async Task DeleteAdvisor_ShouldReturnNotFound_WhenAdvisorDoesNotExist()
    {
        // Act
        var result = await _controller.DeleteAdvisor(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAdvisors_ShouldReturnAllAdvisors()
    {

        var result = await _controller.GetAdvisors();


        // Extract the result from ActionResult<T>
        var okResult = result.Result as OkObjectResult;

        // Ensure the result is an OkObjectResult
        Assert.NotNull(okResult);

        // Extract the list of advisors from the result
        var advisors = okResult.Value as IEnumerable<object>;

        // Ensure the value is a list of objects
        Assert.NotNull(advisors);

        // Ensure the list is not empty
        Assert.NotEmpty(advisors);

        // Assert that the list contains the expected number of advisors
        Assert.Equal(2, advisors.Count());

        // Verify the properties of each advisor in the list
        foreach (var advisor in advisors)
        {
            var id = advisor.GetType().GetProperty("Id").GetValue(advisor, null);
            var name = advisor.GetType().GetProperty("Name").GetValue(advisor, null);
            var sin = advisor.GetType().GetProperty("SIN").GetValue(advisor, null) as string;
            var address = advisor.GetType().GetProperty("Address").GetValue(advisor, null);
            var phone = advisor.GetType().GetProperty("Phone").GetValue(advisor, null) as string;
            var healthStatus = advisor.GetType().GetProperty("HealthStatus").GetValue(advisor, null);

            // Perform assertions
            Assert.NotNull(id);
            Assert.NotNull(name);
            Assert.Matches(@"^\*{5}\d{4}$", sin);  // SIN should be masked as *****XXXX
            Assert.NotNull(address);
            Assert.Matches(@"^\*{4}\d{4}$", phone);  // Phone should be masked as ****XXXX
            Assert.NotNull(healthStatus);
        }
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnBadRequest_WhenNameIsNull()
    {
        // Arrange
        var advisor = new Advisor { SIN = "444555666", Address = "Some Address", Phone = "33445566" };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Name is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnBadRequest_WhenSINIsInvalidLength()
    {
        // Arrange
        var advisor = new Advisor { Name = "Invalid SIN", SIN = "123", Address = "Some Address", Phone = "12345678" };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("SIN must be exactly 9 characters long.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnBadRequest_WhenSINIsNotExactly9Characters()
    {
        // Arrange
        var advisor = new Advisor
        {
            Name = "Test Advisor",
            SIN = "1234", // Invalid SIN length
            Address = "789 Road",
            Phone = "87654321"
        };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("SIN must be exactly 9 characters long.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetAdvisor_ShouldReturnMaskedSINAndPhone_WhenAdvisorExists()
    {
        // Act
        var result = await _controller.GetAdvisor(1);

        if (!(result is OkObjectResult))
        {
            throw new Exception($"Expected OkObjectResult but got {result.GetType().Name}");
        }

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var advisor = okResult.Value;

        Assert.Equal("*****6789", advisor.GetType().GetProperty("SIN").GetValue(advisor, null));
        Assert.Equal("****5678", advisor.GetType().GetProperty("Phone").GetValue(advisor, null));
    }

    //[Fact]
    //public async Task CreateAdvisor_ShouldStoreFullSINAndPhoneCorrectly()
    //{
    //    // Arrange
    //    var advisor = new Advisor
    //    {
    //        Name = "Test Advisor",
    //        SIN = "888888888",
    //        Address = "789 Road",
    //        Phone = "87654321"
    //    };

    //    // Act
    //    var result = await _controller.CreateAdvisor(advisor);

    //    // Assert
    //    var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    //    var createdAdvisor = Assert.IsType<Advisor>(createdAtActionResult.Value);

    //    Assert.Equal("888888888", createdAdvisor.SIN);
    //    Assert.Equal("87654321", createdAdvisor.Phone);
    //}

    [Fact]
    public async Task GetAdvisors_ShouldReturnMaskedSINAndPhone()
    {
        var result = await _controller.GetAdvisors();

        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var advisors = okResult.Value as IEnumerable<object>;
        // Assert
        Assert.NotNull(advisors);

        foreach (var advisor in advisors)
        {
            var sin = advisor.GetType().GetProperty("SIN").GetValue(advisor, null) as string;
            var phone = advisor.GetType().GetProperty("Phone").GetValue(advisor, null) as string;

            Assert.Matches(@"^\*{5}\d{4}$", sin);  // SIN should be masked as *****XXXX
            Assert.Matches(@"^\*{4}\d{4}$", phone);  // Phone should be masked as ****XXXX
        }
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnBadRequest_WhenPhoneIsNotExactly8Characters()
    {
        // Arrange johnson
        var advisor = new Advisor
        {
            Name = "Test Advisor",
            SIN = "999999999",
            Address = "789 Road",
            Phone = "12345" // Invalid Phone length
        };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Phone number must be exactly 8 characters long.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateAdvisor_ShouldStoreUnmaskedSINAndPhone()
    {
        // Arrange
        var advisor = new Advisor
        {
            Id = 1,
            Name = "Updated Name",
            SIN = "987654321",
            Address = "Updated Address",
            Phone = "87654321",
            HealthStatus = "Green"
        };

        // Act
        var result = await _controller.UpdateAdvisor(1, advisor);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var updatedAdvisor = await _context.Advisors.FindAsync(1);
        Assert.Equal("987654321", updatedAdvisor.SIN);
        Assert.Equal("87654321", updatedAdvisor.Phone);
    }

    [Fact]
    public async Task CreateAdvisor_ShouldReturnBadRequest_WhenNameIsWhitespace()
    {
        // Arrange
        var advisor = new Advisor
        {
            Name = "   ", // Invalid Name
            SIN = "123456789",
            Address = "789 Road",
            Phone = "87654321"
        };

        // Act
        var result = await _controller.CreateAdvisor(advisor);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Name is required.", badRequestResult.Value);
    }
}
