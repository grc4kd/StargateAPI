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
2. Could I use this project as a demo in my github publically, provided I make sure the license is followed?
3. I know that having a UI is important, and I can use Angular too, but I might need to prioritize work on the 
   API so that I don't end up with an incomplete project here. Is having some of the API done and some of the UI
   done preferrable to having a completed solution?
4. It looks like were returning InternalServerError for all server errors, even if the requested resource was
   NotFound. Since this is for a GET request only, is NotFound an okay API HTTP status code to use?

# Things I'm skipping due to deadlines
- I see that migrations have warnings about data being dropped unexpectedly due to a rebuild during
  seeding operations, however, I'm putting a pin in it for later.
- I would set up an automation to run code coverage reports but these tools are highly company specific
  and I'm getting by for now by running the built in coverage tests with XPlat and reportgenerator isntead.
  