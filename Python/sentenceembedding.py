from fastapi import APIRouter
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer
from unidecode import unidecode
import re

routerSentence = APIRouter()


print("Model loading...")
model = SentenceTransformer("multi-qa-mpnet-base-dot-v1")
print("Model loaded!")

class DataRequest(BaseModel):
    message: str

@routerSentence.post("/embed")
def embed(req: DataRequest):
    emb = model.encode(normalize(req.message)).tolist()
    return {"embedding" : emb}

def normalize(text:str) -> str:
    #lower chars
    text = text.lower()
    #normalize unicode
    text = unidecode(text)
    #change special chars to space
    text = re.sub(r"[^a-z0-9]+"," ", text)
    #delete extra spaces
    text = text.strip()
    return text