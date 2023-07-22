# JWT_Protected_Function
Practice securing azure functions with JWT Tokens 

# Azure auth Steps
1. Setup an app registraion from azure ad for the function api.
  a. Set up scope access_as_user
  b. Set up app role User.Read
  c. Assign your user the User.Read role in the service principal (Enterprise application).
3. Setup an app registraion from azure ad for the 'web' application.
  a. Add scope access_as_user from function api registraion.

# Debug
1. Run functions application
2. Run Auth Code Grant flow from your app or postman
   a. Authorize
   b. Request access token
   c. Call API with access token
   b. If running from app use functions key


