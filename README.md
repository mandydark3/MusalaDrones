# Musala Drones Project

Solution to Musala Drones project programming task.

## Installation

This project requires Visual Studio 2022 with ASP.NET and Web development components installed.
(Team or Professional version is required to be able to run tests)

Building or running the project requires opening the solution file wich is locate inside the MusalaDrones folder (.sln extension) and press F5 for debug or Ctrl + F5 to run. 
The solution file contains both projects (MusalaDrones - WEB API and MusalaDrones.Tests - Tests). Both projects contain git history.
Running the tests requires using Tests menu and then Run all tests.

Some browsers may indicate problems with the application's certificate, a quick way to solve this problem is to create self-signed certificate to enable HTTPS use in local web app development running. This can be achieved by running the following command in a cmd prompt

dotnet dev-certs https --trust

In the first run, application will automatically create the database as well as some dummy drone and medication data to work on.
Database will be created locally at C:\Users\{current_user}\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB

Drone data
----------------------------------------------------------------------------------------------------------------------------------------------------------------
SerialNumber = "SN1", Model = Lightweight,   WeightLimit = 500, BatteryCapacity = 100, State = IDLE
SerialNumber = "SN2", Model = Middleweight,  WeightLimit = 300, BatteryCapacity = 60,  State = IDLE
SerialNumber = "SN3", Model = Cruiserweight, WeightLimit = 200, BatteryCapacity = 30,  State = IDLE
SerialNumber = "SN4", Model = Heavyweight,   WeightLimit = 400, BatteryCapacity = 15,  State = IDLE

Medication data
----------------------------------------------------------------------------------------------------------------------------------------------------------------
Code = "MED01", Name = "Ativan", 		Weight = 10
Code = "MED02", Name = "Hydroxyzine Pamoate",	Weight = 30
Code = "MED03", Name = "Glucosamine", 		Weight = 20
Code = "MED04", Name = "Clotrimazole", 		Weight = 50
Code = "MED05", Name = "Ubrogepant", 		Weight = 100

## Usage
Each functional requirement of the application is implemented through the uses of the services.
The action can be invoke following by using:

ApplicationURI/api/ActionName

List of services corresponding to each functionality is (Assuming that the application is running with the standard configuration):

[Checking drone battery level for a given drone]
https://localhost:5285/api/GetDroneBatteryLevel

[Checking available drones for loading]
https://localhost:5285/api/GetAvailableDronesForLoading

[Checking loaded medication items for a given drone]
https://localhost:5285/api/GetMedicationFromDrone

[Loading a drone with medication items]
https://localhost:5285/api/LoadDroneWithMedication

[Registering a new drone]
https://localhost:5285/api/RegisterDrone

Every single action is well documented inside the corresponding controller (DroneController.cs)

## Usage (Some examples)

1. [Checking drone battery level for a given drone]
Param: "searchData"
JSON param formed as { "SerialNumber": [Drone serial number] }
Example: {"SerialNumber": "S01"}
Returns: JSON result formed as: { "result": [The actual result] }
Actual result could be the drone battery level or if an error occurred 
Example: {"result": 100}

2. Checking available drones for loading
Param: None
Returns: JSON result formed as: { "result": [The actual result] }
Actual result could be the list containing available drones or if an error occurred 
Example: {"result": [{"SerialNumbre": "SN1", "Model": "Lightweight", "WeightLimit": 500, "BatteryCapacity": 100}, {...}]

3. Checking loaded medication items for a given drone
Param: searchData
JSON param formed as { "SerialNumber": [Drone serial number] }
Example: {"SerialNumber": "S01"}
Returns: JSON result formed as: { "result": [The actual result] }
Actual result could be the list containing medication loaded into the drone or if an error occurred 
Example: {"result": [{"Code": "MED01", "Name": "Ativan", "Weight": 10, "Quantity": 4}, {...}]

4. Loading a drone with medication items
Param: loadingInfo
JSON param formed as { "SerialNumber": [Drone serial number], "Medications": [Pair key-value list formed as "Key": "Code", "Value": Quantity] }
Example: {"SerialNumber": "S01", [{"Key": "MED01", "Value": 3},{"Key": "MED02", ""Value": 1}]}
Returns: JSON result formed as: { "result": [The actual result] }
Actual result could be the list containing medication left, if any (couldn't be loaded into the drone due to drone weight limit) or if an error occurred
Example: {"result": "SerialNumber": "S01", [{"Code": "MED01", "Name": "Ativan", "Weight": 10, "Quantity": 1}, {...}]

5. Registering a new drone
Param: newDrone
JSON param formed as { "SerialNumber": [Drone serial number], "Model": [Number between 1 and 4], "WeightLimit": [Weight limit], "BatteryCapacity": [Battery percent] }
Example: {"SerialNumber": "S010", "Model": 1, "WeightLimit": 500, "BatteryCapacity": 100}
Returns: JSON result formed as: { "result": [The actual result] }
Actual result could be "OK" if the drone could be created or if an error occurred  
Example: {"result": "OK"}

