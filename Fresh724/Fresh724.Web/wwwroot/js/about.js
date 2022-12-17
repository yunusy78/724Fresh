//Resources 
const observer = new IntersectionObserver((entries)=> {
    entries.forEach((entry)=>{

            console.log(entry)
            if(entry.isIntersecting && entry.target.classList.contains('hiddenFade')){
                entry.target.classList.add('showFade');
            }
            else if(entry.isIntersecting && entry.target.classList.contains('hiddenTransition')){
                entry.target.classList.add('showTransition');
            }
            else if(entry.isIntersecting===false && entry.target.classList.contains('showFade')){
                entry.target.classList.remove('hiddenFade');
            }

            else if(entry.isIntersecting===false && entry.target.classList.contains('showTransition'))
                entry.target.classList.remove('hiddenTransition');
        }




    )

})
const elements = document.querySelectorAll('.hiddenFade')
elements.forEach((el)=>observer.observe((el)))
const elements1 = document.querySelectorAll('.hiddenTransition')
elements1.forEach((el)=>observer.observe((el)))