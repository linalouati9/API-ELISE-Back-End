
# API ELISE BACK END
This is the backend code for an API in the Elise GED (Electronic Document Management System) that generates separator sheets with QR codes from a list of documents stored as XML in the GED. This API uses a model containing XSLT codes, which are either manually written or generated using the LLM phi3 language model from Ollama in offline mode.

## Run Locally
To run this project locally follow this steps :
* Clone project
    ``` cmd 
    git clone https://github.com/linalouati9/API-ELISE-Back-End
    ```
* Go to the project directory :
    ``` cmd 
    cd api-elise
    ```
* Run the project
    ``` cmd 
    dotnet run
    ```

## API Reference
1/ QRCode Endpoints
#### Get QR codes 
    GET /api/QRCode

#### Get QR code by id
    GET /api/QRCode/${id}
    
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `id`      | `integer`   | **Required**. Id of the qrcode to fetch
    
#### Get QR code by title

    GET /api/QRCode/${title}
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `title`   | `string`   | **Required**. Title of the qrcode to fetch
 
#### Post QR code 
    POST /api/QRCode
    
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `ModelId` | `integer`   | **Required**. The ID of the model containing this qrcode

    | Request body parameters | Type        | Description
    | :---------------------- | :---------- | :-----------------------
    | `title` | `string`   | **Required**. The title of the QR code
    | `xslt` | `string`   | **Required**. The XSLT code for qrcode generation.

#### Put QR code 
    PUT /api/QRCode/${id}
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `id` | `integer`   | **Required**. The ID of the qrcode to update

    | Request body parameters | Type        | Description
    | :---------------------- | :---------- | :-----------------------
    | `title` | `string`   | **Required**. The new title of the QR code
    | `xslt` | `string`   | **Required**. The new XSLT code for qrcode generation.

#### DELETE QR code 
    DELETE /api/QRCode/${id}
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `id` | `integer`   | **Required**. The ID of the qrcode to delete.


2/ Model Endpoints
#### Get models
    GET /api/Model

#### Get model by id
    GET /api/Model/${id}
    
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `id`      | `integer`   | **Required**. Id of the model to fetch
    
#### Get model by title

    GET /api/Model/${title}
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `title`   | `string`   | **Required**. Title of the model to fetch
 
#### Post model 
    POST /api/Model

    | Request body parameters | Type        | Description
    | :---------------------- | :---------- | :-----------------------
    | `title` | `string`   | **Required**. The title of the model to create
    | `description` | `string`   | **Required**. Describe the model
    | `template` | `string`   | **Required**. The template of the separatorSheet for this model
    | `qrcodes` | `List<QRCodes>`   | **Required**. A model have at least 1 qrcode.

#### Put QR code 
    PUT /api/Model/${id}
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `id` | `integer`   | **Required**. The ID of the model to update

    | Request body parameters | Type        | Description
    | :---------------------- | :---------- | :-----------------------
    | `title` | `string`   | **Required**. The new title of the model to update
    | `description` | `string`   | **Required**. New describtion of the model
    | `template` | `string`   | **Required**. The updated template of the separatorSheet for this model
    | `qrcodes` | `List<QRCodes>`   | **Required**. A model have at least 1 qrcode.

#### DELETE Model 
    DELETE /api/Model/${id}
    | Parameter | Type        | Description
    | :-------- | :---------- | :-----------------------
    | `id` | `integer`   | **Required**. The ID of the model to delete.

## Authors
* Lina LOUATI [lina.louati@fsb.ucar.tn]
* Haythem Hassine JAIDANE [HaythemHassine.JAIDANE@esprit.tn]
* Hadhemi MAHMOUD [hadhemi.mahmoud@esprit.tn ]
* Jihed AYARI [jihedayari492@gmail.com]


