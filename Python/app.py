from fastapi import FastAPI
import uvicorn
from sentenceembedding import routerSentence 

app = FastAPI()

@app.get("/ready")
def health():
    return {"ready": True}

app.include_router(routerSentence)

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)