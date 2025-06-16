import re
import pandas as pd

# user로부터 파일 경로 입력받기
file_path = input("분석할 텍스트 파일 경로를 입력하세요 (.txt 포함): ").strip()

# 정규식 패턴
generation_pattern = r'\[=+Generation (\d+)=+\]'
header_pattern = r'Name\s+:\s+(.*)'
wolf_pattern = r'(Wolf\d+)\s*:\s*(.*)'

# 저장할 데이터
data = []
columns = []
generation = None
mtabs_index = -1  # MTABS 인덱스 추적용

try:
    with open(file_path, 'r', encoding='utf-8') as file:
        for line in file:
            gen_match = re.search(generation_pattern, line)
            if gen_match:
                generation = int(gen_match.group(1))

            header_match = re.search(header_pattern, line)
            if header_match:
                # 헤더 추출
                raw_columns = [col.strip() for col in header_match.group(1).split('|') if col.strip()]
                if "MTABS" in raw_columns:
                    mtabs_index = raw_columns.index("MTABS")
                    raw_columns.pop(mtabs_index)  # MTABS 제거
                columns = ['Generation', 'WolfName'] + raw_columns

            wolf_match = re.search(wolf_pattern, line)
            if wolf_match:
                wolf_name = wolf_match.group(1)
                raw_values = [float(v.strip()) for v in wolf_match.group(2).split('|') if v.strip()]
                if 0 <= mtabs_index < len(raw_values):
                    raw_values.pop(mtabs_index)  # MTABS 값 제거
                data.append([generation, wolf_name] + raw_values)

    # 결과 저장
    if data and columns:
        df = pd.DataFrame(data, columns=columns)
        output_csv = file_path.replace('.txt', '_parsed.csv')
        df.to_csv(output_csv, index=False, encoding='utf-8-sig')
        print(f'CSV 파일이 저장되었습니다: {output_csv}')
    else:
        print("유효한 Wolf 데이터가 파일에서 추출되지 않았습니다.")

except FileNotFoundError:
    print(f"파일을 찾을 수 없습니다: {file_path}")
except Exception as e:
    print(f"오류 발생: {e}")