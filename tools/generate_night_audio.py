from pathlib import Path
import wave,math,random,struct
out=Path(__file__).resolve().parents[1]/'Assets/_Project/Audio/Night';out.mkdir(parents=True,exist_ok=True)
rate=22050
for name,duration in [('Pistol',.22),('Carbine',.16),('Scatter',.4),('Groan',1.4),('Scream',1.3),('Breathing',3.2),('Melee',.18)]:
    rng=random.Random(name); samples=[];low=0
    for i in range(int(rate*duration)):
        t=i/rate; x=t/duration; noise=rng.uniform(-1,1);low=.88*low+.12*noise
        if name in ('Pistol','Carbine','Scatter'):
            decay={'Pistol':24,'Carbine':35,'Scatter':15}[name]; v=(noise*.45+math.sin(2*math.pi*(65*t+45*t*math.exp(-t*30)))*.5)*math.exp(-decay*t)
        elif name=='Groan': v=(math.sin(2*math.pi*(70*t+7*math.sin(t*3)))+.3*math.sin(2*math.pi*137*t)+low)*.22*math.sin(math.pi*x)**2
        elif name=='Scream': v=(math.sin(2*math.pi*(310*t+90*t*t))*.35+low*.9)*math.sin(math.pi*x)**2
        elif name=='Breathing': v=low*.6*math.sin(math.pi*x*2)**2
        else: v=noise*.28*math.sin(math.pi*x)**2
        v*=min(1,t*rate/24); samples.append(struct.pack('<h',int(max(-1,min(1,v))*30000)))
    with wave.open(str(out/(name+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(b''.join(samples))
