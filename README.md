## KekeDataStore.Json

[![NuGet](https://img.shields.io/nuget/v/KekeDataStore.Json.svg)](https://www.nuget.org/packages/KekeDataStore.Json/) [![NuGetCount](https://img.shields.io/nuget/dt/KekeDataStore.Json.svg
)](https://www.nuget.org/packages/KekeDataStore.Json/)

![alt text](https://raw.githubusercontent.com/keke1210/KekeDataStore.Json/master/icon.ico)


Simple data store that saves the data into zipped flat files.

* Small API with basic functionality that is needed for handling CRUD operations.
* Thread-safe.
* Works with strongly typed data
* Data is stored in separated binary files
  * Easy to initialize
  * Fast 
  * Self-contained
  * In-memory db 
  * Easy to edit
  * Good choice for small apps and prototyping
* [.NET Standard 2.0](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
  * .NET Core 3.1 & .NET Framework 4.8

## Installation

You can install the latest version via [NuGet](https://www.nuget.org/packages/KekeDataStore.Json/).

```sh
# Package Manager
PM> Install-Package KekeDataStore.Json -Version 1.0.1.2

# .NET CLI
> dotnet add package KekeDataStore.Json --version 1.0.1.2

# PackageReference
<PackageReference Include="KekeDataStore.Json" Version="1.0.1.2" />
```

## Usage

### Declaring Entities
Data is stored as a Key-Value so each class Entity that you want to be saved in the store, must implement ``IBaseEntity.cs`` interface from ``KekeDataStore.Json`` project.
#### Example
```csharp
// IBaseEntity.cs 
namespace KekeDataStore.Json
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
    }
}
```

```csharp
//Entity (Model)
public class Contact : IBaseEntity
{
    public Guid Id { get; set; }
    public Person Person { get; set; }
    public Phone Phone { get; set; }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Phone
{
    public string PhoneNumber { get; set; }
    public PhoneType Type { get; set; }
}

public enum PhoneType : byte
{
    WORK,
    CELLPHONE,
    HOME
}
```
You can initialize an instance in your method like: 
```csharp
// a file with the file name 'Contacts.bin' will be generated automatically by default, on client's root directory
var dataStore = new JsonDataStore<Contact>();
```
You can manually specify the file path like:
````csharp
// the first parameter specifies the directory path of the file that will be created 
//and the second the name of the 'dbname'.bin file which will be created if it doesn't exists already. 
var dataStore = new JsonDataStore<Contact>(dirPath: @"..\..\Some\Path", dbName: "TestTable");
````
or
````csharp
// When class instatiated like this, the first parameter will take the directory path
// and the file name will be generated by the Entity class in plurar. For ex. in this case 'Contacts.bin' 
var dataStore = new JsonDataStore<Contact>(dirPath: @"..\..\Some\Path")
````

### Example
```csharp
using KekeDataStore.Json;

// Open database (create new file in the bin/Debug directory called "Contacts.bin")
var dataStore = new JsonDataStore<Contact>();

// Create new contact instance
var contact = new Contact { Person = new Person { FirstName = "Skerdi", LastName = "Berberi" },
                            Phone = new Phone { PhoneNumber = "0684555555", Type = PhoneType.HOME } };

// Insert new contact
// Id is generated automatically when it's not specified
dataStore.Create(contact);

// Update contact
contact.Person.FirstName = "TestFirstName";

// Updates the element and then returns the updated element
var updatedContact = dataStore.Update(contact.Id.ToString(), contact);

// Save data to binary file. If you never call SaveChanges() you can use the api like an in-memory database
dataStore.SaveChanges();

// Get all items in the store
var allcontacts = dataStore.GetAll();

// Get all items with strategy design pattern
// the example below shows that you can use it for search functionality
var contactsWithPredicate = dataStore.Get(x => x.Person.FirstName.Contains("Sk")); 

// Get element by Id (Key)
var contactById = dataStore.GetById(contact.Id.ToString());

// Gets single item, with predicate. If the predicate returns more than a single value, it throws an error.
var singleContact = dataStore.GetSingle(x => x.Id == new Guid("bba9dcb0-563e-42b6-a2ff-6554ebba87f2"));

// Orders contacts by person's first name in ascending order, then by lastname ascending
var orderedContacts = dataStore.AsQueryable().OrderBy("Person.FirstName").ThenBy("Person.LastName");

// Orders contacts by person's last name in descending order, then it sorts by firstname descending 
var orderedContacts = dataStore.AsQueryable().OrderByDescending("Person.LastName").ThenBy("Person.FirstName");

// Use LINQ to query items, also you can use LINQ with GetAll, Get ...
var queryContacts = dataStore.AsQueryable().Where(x => x.Person.FirstName == "Skerdi");

// Removes the item from the store
var deleteContact = dataStore.Delete(contact.Id.ToString());

// Removes all items 
dataStore.Truncate();
```

## Note
This library was inspired by ``ttu/json-flatfile-datastore`` :  https://github.com/ttu/json-flatfile-datastore.


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

Licensed under the [MIT](LICENSE) License.