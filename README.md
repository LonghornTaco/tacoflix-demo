# Cortex Processing Engine Demo

## Prerequisites
This demo assumes you're running a full XP installation of Sitecore 9.1 Initial Release.

There are two custom Outcomes that were created for this demo:
* Movie Renter
* Movie Rented

Once you create these outcomes, put the Item IDs in the `app.config` of `TacoFlix.Client`.

You will also need to sign up for a developers API key from TheMovieDB.org.  This is a free service.  Once you receive your API key, you'll need to update the same `app.config` file as above, but also the `sc.TheMovieDb.xml` in `TacoFlix.ProcessingEngine.Extensions/App_Data/Config/Sitecore/Movies`.

## Deployment

First, you'll need to generate the model `json` file.  You can do this through the `TacoFlix.Client`.  Just click the xDB menu item in the top left, then select `Generate Model`.  This will generate the model file in the default output location for `TacoFlix.Client`.  You'll need to copy this model file to four locations:

* `<xConnect-root>/App_Data/Models`
* `<index-worker-root>/App_Data/Models`
* `<processing-engine-root>/App_Data/Models`

Next, copy the `TacoFlix.Model.dll`, `TacoFlix.Xconnect.Model.dll` and `TacoFlix.ProcessingEngine.Extensions.dll` to the following four locations:

* `<xConnect-root>/bin`
* `<index-worker-root>/`
* `<processing-engine-root>/`
* `<sitecore-webroot>/bin`

Third, copy the contents of the `App_Data` folder in `TacoFlix.ProcessingEngine.Extensions` to the `<processing-engine-root>/App_Data` folder.

Lastly, in `TacoFlix.Client/busSettings.xml`, make sure the two database connection strings have the proper information for the `Messaging` database.

### Happy Demoing!
