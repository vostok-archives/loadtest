0. Запустить EventGenerator:

    > dotnet EventGenerator.dll

1. Создать рабочий каталог для танка и перейти в него.  В этом каталоге танк оставит логи

    > md tank
    > cd tank

2. Скопировать в рабочий каталог конфиг танка tank-vostok.yaml.

3. Скопировать в рабочий каталог нужный файл с патронами в файл ammo:

    > cp ammo-logs.txt ammo

4. Запустить танк:

    docker run -v $(pwd):/var/loadtest -v $HOME/.ssh:/root/.ssh --net host -it direvius/yandex-tank -c tank-vostok.yaml

5.  Логи танка будут в каталоге logs.