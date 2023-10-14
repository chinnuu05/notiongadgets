var btn = document.getElementById('button'),
    page = document.getElementById('page'),
    water = document.getElementById('water'),
    cnt = document.getElementById('count'),
    percent = cnt.innerHTML,
    random, diff, interval, isInProgress;

var actual = document.getElementById("actual-value");
var down = document.getElementById("button2");
btn.addEventListener('click', IncreaseLevel);
down.addEventListener('click', DecreaseLevel);

var water_labels = [0, 33, 67, 100];
var water_values = [25, 50, 75, 93];
var water_inner_values = [];
var currentWaterValue;

function IncreaseLevel() {

  // 3 levels
  // 25%, 50%, 90%
  // 75%, 50%, 10%


  if (isInProgress) { return; }
  // btn.removeEventListener('click', IncreaseLevel);
  isInProgress = true;

  var actualValue = parseInt(actual.innerHTML);
  var percent;
  var counter;
  var currentIndex;

  for (let i = 0; i < water_values.length; i++) {
    if (water_values[i] == actualValue) {
      currentIndex = i;
      counter = water_labels[i];

      // set water value to the next value in the list
      if (actualValue != water_values[water_values.length - 1]) {
        percent = water_values[i + 1];
        target = water_labels[i + 1];
        // console.log("new percent set to: " + percent);
      } else { percent = actualValue };
    }
  }
  
  interval = setInterval(function() {
    

    // when water reaches the new increased value, stop
    if (currentIndex == 3) currentIndex = 2;

    if (actualValue == percent && counter == water_labels[currentIndex + 1]) {
      clearInterval(interval);
      // when animation is finished then disable in progress to avoid spams
      isInProgress = false;


    } 

    
    if (actualValue != percent) actualValue += 1;
    if (counter <= 100) counter++;

    // if water level == third level then change text color too
    if (percent == water_values[water_values.length - 1] && actualValue > 85) {
      var literLabel = document.getElementById("liter-label");

      literLabel.classList.add("text-white");


    }
    // console.log("setting water level to: " + (100 - actualValue))

    water.style.transform = 'translate(0, ' + (100 - actualValue) + '%)';
    water.querySelector('.water__inner').style.height = actualValue + '%';
    cnt.innerHTML = counter - 1;

    // isInProgress = false;
    actual.innerHTML = percent;
  }, 16);



}

function DecreaseLevel() {

  // 3 levels
  // 25%, 50%, 90%
  // 75%, 50%, 10%


  if (isInProgress) { return; }
  // btn.removeEventListener('click', DecreaseLevel);
  isInProgress = true;

  var actualValue = parseInt(actual.innerHTML);
  var percent;
  var currentIndex;

  for (let i = 0; i < water_values.length; i++) {
    if (water_values[i] == actualValue) {
      // set water value to the next value in the list
      currentIndex = i;
      counter = water_labels[i];

      if (actualValue != water_values[0]) {
        percent = water_values[i - 1];
        // console.log("new percent set to: " + percent);
      } else { percent = actualValue };
    }
  }

  interval = setInterval(function() {
    
    if (currentIndex == 0) currentIndex = 1;


    if (actualValue == percent && counter == water_labels[currentIndex - 1]) {

      counter += 1;
      clearInterval(interval);
      // when animation is finished then disable in progress to avoid spams
      isInProgress = false;
      

    } 


    // if water level less than third oclor
    if (actualValue < water_values[water_values.length - 1]) {
      var literLabel = document.getElementById("liter-label");

      literLabel.classList.remove("text-white");


    }

    if (actualValue != percent) actualValue -= 1;
    if (counter > 0) counter--;
    // console.log("setting water level to: " + (100 - actualValue))

    water.style.transform = 'translate(0, ' + (100 - actualValue) + '%)';
    water.querySelector('.water__inner').style.height = actualValue + '%';

    // isInProgress = false;
    actual.innerHTML = percent;

    
    cnt.innerHTML = counter;
  }, 16);

  console.log(water.querySelector('.water__inner').style.height);
  if (water.querySelector('.water__inner').style.height == "25%") {
    console.log("changing to 50");
    water.querySelector('.water__inner').style.height = "50%";
    console.log(water.querySelector('.water__inner').style.height);

  }
}
