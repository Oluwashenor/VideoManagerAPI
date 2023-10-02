# Documentation for the video Uploader

## Upload Video



- **URL** - http://oluwashenor-001-site1.ftempurl.com/api/startStream

This api is called to start any recording , it reutns the ID for the video, so subscequent calls would wuse this ID.
- **Method** - Get

**Response**
```
{
  "data": "6480ed82-6f55-43db-aaee-d4cf0f213c42",
  "status": true,
  "message": "Successful Operation"
}
```

- **URL** - http://oluwashenor-001-site1.ftempurl.com/api/stopStream/{id}

This api is called to stop any recording
- **Method** - Get

**params**
```
id : "id value"
```


**Response**
```
{
  "data": [
    {
      "chunk": "string",
      "chunkString": "string",
      "id": "string",
      "created": "2023-10-02T11:32:46.546Z"
    }
  ],
  "status": true,
  "message": "string"
}
```

- **URL** - http://oluwashenor-001-site1.ftempurl.com/api/getStream/{id}

This mapi is called to get any recording
- **Method** - Get

**params**
```
id : "id value"
```


**Response**
```
{
  "data": {
    "id": "string",
    "url": "string",
    "transcripts": [
      {
        "id": 0,
        "text": "string",
        "start": {
          "ticks": 0,
          "days": 0,
          "hours": 0,
          "milliseconds": 0,
          "minutes": 0,
          "seconds": 0,
          "totalDays": 0,
          "totalHours": 0,
          "totalMilliseconds": 0,
          "totalMinutes": 0,
          "totalSeconds": 0
        },
        "end": {
          "ticks": 0,
          "days": 0,
          "hours": 0,
          "milliseconds": 0,
          "minutes": 0,
          "seconds": 0,
          "totalDays": 0,
          "totalHours": 0,
          "totalMilliseconds": 0,
          "totalMinutes": 0,
          "totalSeconds": 0
        },
        "videoId": "string"
      }
    ]
  },
  "status": true,
  "message": "string"
}
```
