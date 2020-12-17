from datetime import datetime
import json

data = '{"water_0": 5,"piezo_0": [1, 2, 3, 4, 5], "piezo_1": [6, 7, 8, 9, 10]}'

print(data)
# Declare Date and Time
date = datetime.now().strftime('%Y-%m-%d')
time = datetime.now().strftime('%H:%M:%S')

# Format Date and Time to JSON
json_date = {"date":date}
json_time = {"time":time}

# Parse Data
json_data = json.loads(data)

print(json_data)
# Add Date and Time Element to JSON Data
json_data.update(json_date)
json_data.update(json_time)

# Generate Vibration Value
vibration1 = sum(json_data["piezo_0"])
vibration2 = sum(json_data["piezo_1"])

#  Format Vibration Values to JSON
json_vibration1 = {"vibration_0":vibration1}
json_vibration2 = {"vibration_1":vibration2}

# Add Vibration Value to JSON Data
json_data.update(json_vibration1)
json_data.update(json_vibration2)

data = json_data
print(data)
#print(json_data)
#write_json(data)

