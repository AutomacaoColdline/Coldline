FROM ubuntu:20.04
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y curl zip unzip && rm -rf /var/lib/apt/lists/*
RUN useradd -ms /bin/bash sdkuser
USER sdkuser
WORKDIR /home/sdkuser
RUN curl -s "https://get.sdkman.io" | bash
ENV SDKMAN_DIR=/home/sdkuser/.sdkman
RUN bash -c "source \$SDKMAN_DIR/bin/sdkman-init.sh && sdk install java 8.0.292-zulu && sdk install java 9.0.4-open"
CMD ["bash"]
