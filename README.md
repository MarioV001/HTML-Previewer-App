# HTML-Previewer-App

Usage of the Controller:
Example use on __"Localhost"__ network:  by typing in the url "https://localhost:44393/" you will automatilcly be redirected to the correct Controller and the new ID session.

> ## Controller Rules:
> There are 3 type of Controller Rules but the main is
> ###### ``` defaults: new { controller = "Run", action = "Index", URLGen = UrlParameter.Optional }```

## The Page consists of total 6 buttons
* **RUN**
  * ###### Pressing the RUN Button It Simply Shows the current HTML Text in the preview. Some Temp File Might be stored as: ```Edit- P65V9BUO -TEMP.htlm```
* **SAVE**
  * ###### This Will Save the Text on the Editor, it will save it into a folder on the Server called “HtmlFiles” File Parameters Example: ```Edit-P65V9BUO.html``` At the same time the Database is updated with the necessary info.

* **EDIT**
   * ###### The Edit Button will generate a new Unique ID and will COPY/Duplicate of the current page and Save it as a NEW Unique ID Page.
But at the same time, linking the previous (Original) file in the Database when saving this new Edit.
* **SHARE**
   * ###### This will simply Show this Session ID so that other Users can access this File. The ID will be shown in the Footer field.
* **CHECK ORIGINAL**
   * ###### If this button is visible, it means that the server has found a Previous Edit before the current one that the user is browsing.
* **NEXT EDIT**
   * ###### If this button is visible, it means that the server found an Edit After the current one that the user is browsing. 
* **SETTINGS**
  * Enter HTML URL
  * Generate NEW URL
  * Close



> ## Database Construct:
> ```ID``` ```HtmlFileID``` ```Save``` ```LastEditTIme``` ```BeforeEditChild``` ```AfterEditChildID``` ```ISOriginal```
