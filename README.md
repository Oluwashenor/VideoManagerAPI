
# Streaming Manager API

This project is an API built using C# ASP.Net Core, it connects to frontend applications where recordings are being sent in real time to the API. 


## Features

- Can Receive videos in chunks of byte array or base64string
- Can transcribe videos to text
- Persistent Storage for transcriptions and videos 
- Fast and Efficient


## API Reference

#### Start Stream

```http
  GET /api/StartStream
```

#### Response
```
{
  "data": "f24dbfcb-a312-47f6-b485-dcb659da560e",
  "status": true,
  "message": "Successful Operation"
}
```

#### Stop Stream

```http
  GET /api/StopStream/${id}
```

#### Response
```
{
    "data": {
        "id": "e1e4caf9-7fc4-412d-886f-2f69eec4124a",
        "url": "e1e4caf9-7fc4-412d-886f-2f69eec4124a.mp4",
        "transcripts": null
    },
    "status": true,
    "message": "Successful Operation"
}
```
| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `id`      | `string` | **Required**. Id of video to stop |

#### Retrieve a video

```http
  GET /api/GetStream/${id}
```

#### Response
```
{
  "data": {
    "id": "e1e4caf9-7fc4-412d-886f-2f69eec4124a",
    "url": "Path/to/file",
    "transcripts": []
  },
  "status": true,
  "message": "Successful Operation"
}
```
| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `id`      | `string` | **Required**. Id of video to Get |




#### Upload Stream in base64 string

```http
  Post /api/UploadStreamInString/
```

#### Request Body
```
{
  "chunkString": "string",
  "id": "string",
  "created": "2023-10-23T10:39:53.953Z"
}
```

#### Response 
```
{
    "data": "Streamed chunk received and saved successfully",
    "status": true,
    "message": "Successful Operation"
}
```

#### Upload Stream in Byte Array

```http
  Post /api/items/${id}
```

#### Request Body
```

```

#### Response 
```
{
    "data": "Streamed chunk received and saved successfully",
    "status": true,
    "message": "Successful Operation"
}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `id`      | `string` | **Required**. Id of video to save |



## Authors

- [@oluwashenor](https://www.github.com/oluwashenor)