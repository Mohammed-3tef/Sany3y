from fastapi import FastAPI, UploadFile, File, HTTPException, Form
from ultralytics import YOLO
import io
from PIL import Image

app = FastAPI(title="OCR YOLO ID Card API")

# تحميل النماذج
id_card_model = YOLO("detect_id_card.pt")
arabic_numbers_model = YOLO("detect_arabic_numbers.pt")

# -----------------------
# دالة لمعالجة الصورة
# -----------------------
def load_image(file: UploadFile):
    image = Image.open(io.BytesIO(file.file.read())).convert("RGB")
    return image

# -----------------------
# endpoint /predict/ لاستخراج الرقم القومي
# -----------------------
@app.post("/predict/")
async def predict(file: UploadFile = File(...)):
    if file is None:
        raise HTTPException(status_code=400, detail="No file uploaded.")

    # تحميل الصورة
    image = load_image(file)

    # توقع صندوق الرقم القومي على البطاقة
    id_results = id_card_model.predict(image)

    extracted_number = ""

    for result in id_results:
        for box in result.boxes:
            x1, y1, x2, y2 = map(int, box.xyxy[0])
            cropped = image.crop((x1, y1, x2, y2))

            # توقع الأرقام داخل المنطقة
            num_results = arabic_numbers_model.predict(cropped)

            # ترتيب الصناديق حسب الإحداثيات (من اليسار لليمين)
            numbers = []
            for nres in num_results:
                for nbox in nres.boxes:
                    numbers.append({
                        "class": int(nbox.cls),
                        "x": float(nbox.xyxy[0][0])
                    })

            numbers = sorted(numbers, key=lambda x: x["x"])
            # دمج الأرقام كرقم قومي واحد
            extracted_number += "".join(str(n["class"]) for n in numbers)

    return {"id_number": extracted_number}
