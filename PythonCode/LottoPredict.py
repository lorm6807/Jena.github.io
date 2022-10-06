# 데이터 다운로드
import numpy as np
rows = np.loadtxt("c:\debug\lotto\lottoNum_.csv",delimiter=",")
row_count = len(rows)

# 당첨번호를 원핫인코딩 벡터로 변환
def numbers2ohbin(numbers):
    ohbin = np.zeros(45) # 45개의 빈칸을 만듦

    for i in range(6): # 여섯개의 당첨번호에 대해서 반복함
        ohbin[int(numbers[i])-1] = 1 # 로또 번호가 1부터 시작하지만 벡터의 인덱스 시작은 0부터 시작하므로 1을 뺌
        
        return ohbin

# 원핫인코딩 벡터 (ohbin)을 번호로 변환
def ohbin2numbers(ohbin):

    numbers=[]

    for i in range(len(ohbin)):
        if ohbin[i] == 1.0: # 1.0으로 설정되어 있으면 해당 번호를 반환값에 추가한다
            numbers.append(i+1)

    return numbers

numbers = rows[:, 1:7]
ohbins = list(map(numbers2ohbin, numbers))

x_samples = ohbins[0:row_count-1]
y_samples = ohbins[1:row_count]


train_idx = (0,800)
val_idx = (801,900)
text_idx = (901, len(x_samples))


import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
from  tensorflow.keras import models

# 모델 정의하기
model = keras.Sequential([
    keras.layers.LSTM(128,batch_input_shape=(1,1,45),return_sequences=False, stateful=True),
    keras.layers.Dense(45,activation='sigmoid')
])

model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

#매 에포크마다 훈련과 검증의 손실 및 정확도를 기록하기 위한 변수

train_loss= []
train_acc=[]
val_loss=[]
val_acc= []


# 최대 100번 에포크까지 수행
for epoch in range(100):
    
    model.reset_states() # 중요! 매 에포크마다 1회부터 다시 훈력하므로 상태 초기화 필요
    
    batch_train_loss = []
    batch_train_acc = []
    
    for i in range(train_idx[0],train_idx[1]):
        xs = x_samples[i].reshape(1,1,45)
        ys=y_samples[i].reshape(1,45)
        
        loss, acc= model.train_on_batch(xs,ys) #배치만큼 모델에 학습시킴
        
        batch_train_loss.append(loss)
        batch_train_acc.append(acc)
        
    train_loss.append(np.mean(batch_train_loss))
    train_acc.append(np.mean(batch_train_acc))
    
    batch_val_loss=[]
    batch_val_acc=[]
    
    for i in range(val_idx[0], val_idx[1]):
        xs = x_samples[i].reshape(1,1,45)
        ys = y_samples[i].reshape(1,45)
        
        loss, acc = model.test_on_batch(xs,ys) # 배치만큼 모델에 입력하여 나온 답을 정답과 비교함
        
        batch_val_loss.append(loss)
        batch_val_acc.append(acc)
    
    val_loss.append(np.mean(batch_val_loss))
    val_acc.append(np.mean(batch_train_acc))

print('epoch {0:4d} train acc {1:0.3f} loss {2:0.3f} val acc {3:0.3f} loss {4:0.3f}'.format(epoch, np.mean(batch_train_acc), np.mean(batch_train_loss), np.mean(batch_val_acc), np.mean(batch_val_loss)))


def gen_numbers_from_probability(numbs_prob):
    ball_box = []
    
    for n in range(45):
        ball_count = int(numbs_prob[n] * 100 +1)
        ball = np.full((ball_count),n+1)
        ball_box +=list(ball)
    
    selected_balls = []
    
    while True:
        
        if len(selected_balls) == 6:
            break
        
        ball_index = np.random.randint(len(ball_box),size=1)[0]
        ball = ball_box[ball_index]
        
        if ball not in selected_balls:
            selected_balls.append(ball)
            
    return selected_balls
    
print('receive numbers')

xs = x_samples[-1].reshape(1,1,45)

ys_pred = model.predict_on_batch(xs)

list_numbers = []

for n in range(5):
    numbers = gen_numbers_from_probability(ys_pred[0])
    numbers.sort()
    print('{0} : {1}'.format(n, numbers))
    list_numbers.append(numbers)
    


