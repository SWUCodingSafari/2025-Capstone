import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.font_manager as fm
import os

# 한글 폰트 설정 (Windows: Malgun Gothic)
plt.rcParams['font.family'] = 'Malgun Gothic'
plt.rcParams['axes.unicode_minus'] = False  # 깨짐 방지

try:
    #CSV 파일 불러오기
    csv_path = input("분석할 csv 파일 경로를 입력하세요 (.csv 포함): ").strip()
    
    if not os.path.isfile(csv_path):
        raise FileNotFoundError(f"파일을 찾을 수 없습니다: {csv_path}")
    
    df = pd.read_csv(csv_path)

    # 필수 컬럼 확인
    if 'WolfName' not in df.columns or 'Generation' not in df.columns:
        raise ValueError("CSV 파일에 'WolfName' 또는 'Generation' 컬럼이 없습니다.")

    # 그래프 이미지 저장 경로 설정
    base_output_dir = r"C:\Users\user\AppData\LocalLow\DefaultCompany\WolfSimulation\wolf_graphs"
    csv_filename = os.path.splitext(os.path.basename(csv_path))[0]
    output_dir = os.path.join(base_output_dir, csv_filename)
    os.makedirs(output_dir, exist_ok=True)

    # WolfName 별 그래프 생성
    grouped = df.groupby('WolfName')
    count = 0  # 저장된 그래프 수 확인용

    for name, group in grouped:
        plt.figure(figsize=(12, 6))
        
        for column in group.columns:
            if column not in ['Generation', 'WolfName']:
                plt.plot(group['Generation'], group[column], label=column)
        
        plt.title(f"{name}의 세대 별 지표 변화")
        plt.xlabel("Generation")
        plt.ylabel("값")
        plt.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
        plt.tight_layout()
        plt.grid(True)
        
        save_path = os.path.join(output_dir, f"{name}_stats.png")
        plt.savefig(save_path)
        plt.close()
        count += 1

    if count > 0:
        print(f"{count}개의 그래프가 저장되었습니다.")
    else:
        print("저장할 그래프가 없습니다. CSV 파일에 데이터가 없을 수 있습니다.")

except FileNotFoundError as fe:
    print(fe)
except pd.errors.EmptyDataError:
    print("CSV 파일이 비어 있습니다.")
except ValueError as ve:
    print(f"데이터 오류: {ve}")
except Exception as e:
    print(f"오류 발생: {e}")