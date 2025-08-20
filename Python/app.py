from fastapi import FastAPI
import uvicorn
from pydantic import BaseModel

app = FastAPI()

class DataRequest(BaseModel):
    message: str

@app.get("/ready")
def health():
    return {"ready": True}

@app.post("/process")
def process_data(data: DataRequest):
    response = f"Received: {data.message}"
    return {"response": response}

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)