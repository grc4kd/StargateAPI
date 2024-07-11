# GRC
# Things I found during the coding exercise

# Dapper SQL Parameter support
Dapper doesn't use interpolation strings from C# to validate SQL ADO parameters as it does with the DynamicParameters 
and Anonymous Parameters. To avoid a SQL injection vulnerability, one of these methods should be called. Since the types 
of the anonymous parameter properties are shared with ADO.NET, many types of SQL injection attacks involving manipulating 
the resulting query text sent to the database can be avoided here.

-- Changed GetPersonByName.cs for this example.
-- Changed other classes as they were found, some indicating a need to escape ' with \'

# A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person.
This rule describes a mutative operation performed by a client when adding a new AstronautDuty record, but I want
to make sure that the conflicting check in CreateAstronautDutyPreProcessor.Process() is safe to remove from the
application. There is a check that throws a BadRequestHttpException when before the CreateAstronautDutyHandler
is able to end date the previous assignment.

I could also change the program to run this verification after the previous AstronautDuty record has been updated
in the database using a IRequestPostProcessor (instead of IRequestPreProcessor), but I'm not sure how this would
benefit the operation at this time.

# Questions to ask?
1. How should I deploy this, is a private github okay?
   public github is fine
2. Could I use this project as a demo in my github publically, provided I make sure the license is followed?
   Yes. It's GPL. 
3. I know that having a UI is important, and I can use Angular too, but I might need to prioritize work on the 
   API so that I don't end up with an incomplete project here. Is having some of the API done and some of the UI
   done preferrable to having a completed solution?
   my call
4. It looks like were returning InternalServerError for all server errors, even if the requested resource was
   NotFound. Since this is for a GET request only, is NotFound an okay API HTTP status code to use?
   my call, makes sense

# Things I'm skipping (for now) due to deadlines
- I see that migrations have warnings about data being dropped unexpectedly due to a rebuild during
  seeding operations, however, I'm putting a pin in it for later.
- I would set up an automation to run code coverage reports but these tools are highly company specific
  and I'm getting by for now by running the built in coverage tests with XPlat and reportgenerator isntead.
- I have an action filter that works to log success and exceptions during API request handling, using the controller
  as an extension point. This does work well to log each request, but it is not set up to send the logs to a database.
- I have a skeleton of a web UI quickly prototyped in Angular 18. It looks like there is some work to do on marshalling
  the data into proper format, but I am able to at least get the data about names to display. This is something
  that I plan to complete to the point of submitting a duty request and displaying all of the available data.

# Notes for demo
- The logs show only exceptions by default. To turn on the logs for successful requests, 
  change `"StargateApi": "Information"` to `"StargateApi": "Trace"` in `appsettings.Development.json` in
  the API project, folder name `StargateAPI`.
- I wanted to demonstrate that I have full stack experience, but I focused heavily on the API side of things. This
  is typically how I would split up my time professionally too. I find that the API for applications like these
  performs most of the complex behavior and is worth getting right up front. Developing an API against a fully
  developed UI can be done, but shimming in the data exchange often results in a mismatch and lack of conformity
  between the user interface and the back end.
- The tests show good coverage, but I didn't want to change the system under test to be anything so different it
  took a long time to figure out. Most of the tests are in `StargateApiTests/App/WebApplicationTests.cs` and are
  integration tests using a wrapper class around `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory`. This
  approach allowed me to even keep the internal members of the API project without changing any access modifiers
  for `Program.cs`, but the partial class approach would work well too. I just like having the two concerns to be
  separated, as the partial class approach adds code only for testing.
